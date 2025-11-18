using Confluent.Kafka;
using System.Text.Json;
using UniversityManagement.AuditService.Models;

namespace UniversityManagement.AuditService.Services;

public interface IKafkaProducerService
{
    Task<bool> PublishAuditEventAsync(AuditEvent auditEvent);
}

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly string _auditTopic;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _auditTopic = _configuration["Kafka:AuditTopic"] ?? "university.audit.events";

        var config = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.Leader,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            DeliveryReportFields = "all"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task<bool> PublishAuditEventAsync(AuditEvent auditEvent)
    {
        try
        {
            var message = JsonSerializer.Serialize(auditEvent);
            var kafkaMessage = new Message<string, string>
            {
                Key = $"{auditEvent.EntityName}:{auditEvent.EntityId}",
                Value = message,
                Headers = new Headers()
                {
                    { "eventType", System.Text.Encoding.UTF8.GetBytes(auditEvent.EventType) },
                    { "entityName", System.Text.Encoding.UTF8.GetBytes(auditEvent.EntityName) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(auditEvent.Timestamp.ToString("O")) }
                }
            };

            var result = await _producer.ProduceAsync(_auditTopic, kafkaMessage);
            
            _logger.LogInformation("Evento publicado correctamente. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}", 
                result.Topic, result.Partition, result.Offset);
            
            return true;
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Fallo en la publicaicon de kafka. Error: {ErrorCode}, Reason: {Reason}", 
                ex.Error.Code, ex.Error.Reason);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en la publicacion de kafka");
            return false;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}