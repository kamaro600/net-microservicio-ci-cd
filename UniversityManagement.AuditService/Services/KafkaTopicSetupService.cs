using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace UniversityManagement.AuditService.Services;

/// <summary>
/// Servicio para configurar automáticamente los topics de Kafka al iniciar el AuditService
/// </summary>
public class KafkaTopicSetupService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaTopicSetupService> _logger;
    private readonly string _auditTopic;
    private readonly string _deadLetterTopic;
    private readonly string _bootstrapServers;

    public KafkaTopicSetupService(IConfiguration configuration, ILogger<KafkaTopicSetupService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _auditTopic = configuration["Kafka:AuditTopic"] ?? "university.audit.events";
        _deadLetterTopic = configuration["Kafka:DeadLetterTopic"] ?? "university.audit.events.dlq";
        _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configurando topics de Kafka para AuditService");
        _logger.LogInformation("Bootstrap Servers: {BootstrapServers}", _bootstrapServers);
        _logger.LogInformation("Audit Topic: {AuditTopic}", _auditTopic);

        _ = Task.Run(async () =>
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, timeoutCts.Token);

                await CreateTopicsIfNotExistAsync(combinedCts.Token);
                _logger.LogInformation("Topics de Kafka configurados correctamente para AuditService");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Configuración de topics de Kafka cancelada durante el shutdown");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout en configuración de topics de Kafka - continuando sin bloquear la aplicación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar topics de Kafka: {Error} - el servicio continuará funcionando", ex.Message);
            }
        }, cancellationToken);

        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Servicio de configuración de Kafka detenido");
        return Task.CompletedTask;
    }

    private async Task CreateTopicsIfNotExistAsync(CancellationToken cancellationToken)
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _bootstrapServers,
            SecurityProtocol = SecurityProtocol.Plaintext
        };

        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        try
        {
            _logger.LogInformation("Conectando a Kafka en: {BootstrapServers}", _bootstrapServers);
           
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet();

            _logger.LogInformation("Topics existentes en Kafka: {Topics}", string.Join(", ", existingTopics));

            var topicsToCreate = new List<TopicSpecification>();

            if (!existingTopics.Contains(_auditTopic))
            {
                topicsToCreate.Add(new TopicSpecification
                {
                    Name = _auditTopic,
                    NumPartitions = 3, 
                    ReplicationFactor = 1, 
                    Configs = new Dictionary<string, string>
                    {
                        { "retention.ms", "604800000" }, // 7 días
                        { "compression.type", "snappy" },
                        { "max.message.bytes", "1048576" } // 1MB
                    }
                });

                _logger.LogInformation("Topic '{Topic}' será creado", _auditTopic);
            }
            else
            {
                _logger.LogInformation("Topic '{Topic}' ya existe", _auditTopic);
            }

            // Topic de Dead Letter Queue
            if (!existingTopics.Contains(_deadLetterTopic))
            {
                topicsToCreate.Add(new TopicSpecification
                {
                    Name = _deadLetterTopic,
                    NumPartitions = 1, // Una sola partición para DLQ
                    ReplicationFactor = 1,
                    Configs = new Dictionary<string, string>
                    {
                        { "retention.ms", "2592000000" }, // 30 días para investigar errores
                        { "compression.type", "snappy" }
                    }
                });

                _logger.LogInformation("Topic '{Topic}' será creado", _deadLetterTopic);
            }
            else
            {
                _logger.LogInformation("Topic '{Topic}' ya existe", _deadLetterTopic);
            }

            // Crear topics si hay alguno pendiente
            if (topicsToCreate.Any())
            {
                _logger.LogInformation("Creando {Count} topics nuevos", topicsToCreate.Count);
                await adminClient.CreateTopicsAsync(topicsToCreate);
                
                foreach (var topic in topicsToCreate)
                {
                    _logger.LogInformation("Topic '{Topic}' creado exitosamente con {Partitions} particiones", 
                        topic.Name, topic.NumPartitions);
                }
            }
            else
            {
                _logger.LogInformation("Todos los topics ya existen. No se requiere crear nuevos topics.");
            }

            // Verificar que los topics fueron creados correctamente
            await VerifyTopicsAsync(adminClient, cancellationToken);
        }
        catch (CreateTopicsException ex)
        {
            _logger.LogError("Error al crear topics: {Error}", ex.Message);
            
            foreach (var result in ex.Results)
            {
                if (result.Error.Code != ErrorCode.TopicAlreadyExists)
                {
                    _logger.LogError("Error creando topic '{Topic}': {Error}", 
                        result.Topic, result.Error.Reason);
                }
                else
                {
                    _logger.LogInformation("Topic '{Topic}' ya existe", result.Topic);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al configurar topics de Kafka: {Error}", ex.Message);
        }
    }

    private async Task VerifyTopicsAsync(IAdminClient adminClient, CancellationToken cancellationToken)
    {
        try
        {
            // Esperar un poco para que los topics se propaguen
            await Task.Delay(2000, cancellationToken);

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var topics = metadata.Topics.ToDictionary(t => t.Topic, t => t);

            // Verificar topic principal
            if (topics.TryGetValue(_auditTopic, out var auditTopic))
            {
                _logger.LogInformation("Topic '{Topic}' verificado - Particiones: {Partitions}", 
                    _auditTopic, auditTopic.Partitions.Count);
            }
            else
            {
                _logger.LogWarning("Topic '{Topic}' no encontrado después de la creación", 
                    _auditTopic);
            }

            // Verificar DLQ
            if (topics.TryGetValue(_deadLetterTopic, out var dlqTopic))
            {
                _logger.LogInformation("Topic '{Topic}' verificado - Particiones: {Partitions}", 
                    _deadLetterTopic, dlqTopic.Partitions.Count);
            }
            else
            {
                _logger.LogWarning("Topic '{Topic}' no encontrado después de la creación", 
                    _deadLetterTopic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo verificar los topics: {Error}", ex.Message);
        }
    }
}