using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UniversityManagement.NotificationService.Configuration;
using UniversityManagement.NotificationService.Models;

namespace UniversityManagement.NotificationService.Services;

/// <summary>
/// Servicio para gestionar la conexión a RabbitMQ
/// </summary>
public class RabbitMQConnectionService : IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQConnectionService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new object();

    public RabbitMQConnectionService(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQConnectionService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar el exchange
            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            // Declarar las colas
            _channel.QueueDeclare(
                queue: _settings.EnrollmentQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: _settings.UnenrollmentQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // Bind las colas al exchange
            _channel.QueueBind(
                queue: _settings.EnrollmentQueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.EnrollmentRoutingKey
            );

            _channel.QueueBind(
                queue: _settings.UnenrollmentQueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.UnenrollmentRoutingKey
            );

            _logger.LogInformation("Conexion RabbitMQ inicializado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al iniciar RabbitMQ");
            throw;
        }
    }

    public IModel GetChannel()
    {
        lock (_lock)
        {
            if (_channel == null || _channel.IsClosed)
            {
                InitializeRabbitMQ();
            }
            return _channel!;
        }
    }

    public Task PublishNotificationAsync(NotificationRequest notification)
    {
        return Task.Run(() =>
        {
            try
            {
                var channel = GetChannel();
                var routingKey = notification.NotificationType == "Enrollment" 
                    ? _settings.EnrollmentRoutingKey 
                    : _settings.UnenrollmentRoutingKey;

                var messageBody = JsonSerializer.Serialize(notification, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var body = Encoding.UTF8.GetBytes(messageBody);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = notification.MessageId;
                properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)notification.CreatedAt).ToUnixTimeSeconds());
                properties.ContentType = "application/json";

                channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation("Mensaje de notificacion publicado ID: {MessageId}", notification.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAllo en publicacion de mensaje ID: {MessageId}", notification.MessageId);
                throw;
            }
        });
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}