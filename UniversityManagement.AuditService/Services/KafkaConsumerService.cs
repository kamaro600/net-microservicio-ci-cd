using Confluent.Kafka;
using System.Text.Json;
using UniversityManagement.AuditService.Data;
using UniversityManagement.AuditService.Entities;
using UniversityManagement.AuditService.Models;

namespace UniversityManagement.AuditService.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _auditTopic;
    private readonly string _deadLetterTopic;

    public KafkaConsumerService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<KafkaConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _auditTopic = configuration["Kafka:AuditTopic"] ?? "university.audit.events";
        _deadLetterTopic = configuration["Kafka:DeadLetterTopic"] ?? "university.audit.events.dlq";

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = configuration["Kafka:GroupId"] ?? "audit-service-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SessionTimeoutMs = 6000,
            HeartbeatIntervalMs = 3000,
            MaxPollIntervalMs = 300000
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka consumidor error: {Reason}", e.Reason))
            .SetLogHandler((_, message) => _logger.LogInformation("Kafka log: {Message}", message.Message))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        try
        {
            _consumer.Subscribe(_auditTopic);
            _logger.LogInformation("Kafka consumidor inicio. Suscripcion al topico: {Topic}", _auditTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
                    
                    if (consumeResult?.Message != null)
                    {
                        await ProcessAuditEventAsync(consumeResult, stoppingToken);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error de consumo del mensaje: {Error}", ex.Error.Reason);
                    
                    // Si es un error de conexión, esperar antes de reintentar
                    if (ex.Error.Code == Confluent.Kafka.ErrorCode.BrokerNotAvailable ||
                        ex.Error.Code == Confluent.Kafka.ErrorCode.NetworkException)
                    {
                        _logger.LogWarning("Error en la conexion , reintentando...");
                        await Task.Delay(10000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado en kafka");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Servicio cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general");
        }
        finally
        {
            try
            {
                _consumer.Close();
                _logger.LogInformation("Kafka consumidor cerrado");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al cerrar kafka");
            }
        }
    }

    private async Task ProcessAuditEventAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        try
        {
            var auditEvent = JsonSerializer.Deserialize<AuditEvent>(consumeResult.Message.Value);
            
            if (auditEvent == null)
            {
                _logger.LogWarning("Fallo en deserializar el mensaje: {Key}", consumeResult.Message.Key);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EventType = auditEvent.EventType,
                EntityName = auditEvent.EntityName,
                EntityId = auditEvent.EntityId,
                Action = auditEvent.Action,
                UserId = auditEvent.UserId,
                UserName = auditEvent.UserName,
                OldValues = auditEvent.OldValues,
                NewValues = auditEvent.NewValues,
                AdditionalData = auditEvent.AdditionalData,
                Timestamp = auditEvent.Timestamp,
                IpAddress = auditEvent.IpAddress,
                UserAgent = auditEvent.UserAgent
            };

            dbContext.AuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Evento procesado y guardado en BD. EventType: {EventType}, EntityName: {EntityName}, EntityId: {EntityId}", 
                auditEvent.EventType, auditEvent.EntityName, auditEvent.EntityId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Fallo en la deserializacion delmesnaje: {Key}", consumeResult.Message.Key);
            // TODO: Send to dead letter queue
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo del proceso: {Key}", consumeResult.Message.Key);
            // TODO: Send to dead letter queue
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}