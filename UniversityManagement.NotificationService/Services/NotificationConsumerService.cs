using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UniversityManagement.NotificationService.Configuration;
using UniversityManagement.NotificationService.Models;

namespace UniversityManagement.NotificationService.Services;

/// <summary>
/// Servicio consumidor de RabbitMQ que procesa mensajes y envía emails
/// </summary>
public class NotificationConsumerService : BackgroundService
{
    private readonly RabbitMQConnectionService _connectionService;
    private readonly RabbitMQSettings _settings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<NotificationConsumerService> _logger;
    private IModel? _channel;

    public NotificationConsumerService(
        RabbitMQConnectionService connectionService,
        IOptions<RabbitMQSettings> settings,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<NotificationConsumerService> logger)
    {
        _connectionService = connectionService;
        _settings = settings.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio Consumidor de RabbitMQ para NotificationService iniciado");

        try
        {
            _channel = _connectionService.GetChannel();

            // Configurar el consumidor para matrículas
            var enrollmentConsumer = new EventingBasicConsumer(_channel);
            enrollmentConsumer.Received += async (model, ea) =>
            {
                await ProcessEnrollmentMessage(ea);
            };

            // Configurar el consumidor para desmatrículas
            var unenrollmentConsumer = new EventingBasicConsumer(_channel);
            unenrollmentConsumer.Received += async (model, ea) =>
            {
                await ProcessUnenrollmentMessage(ea);
            };

            // Iniciar el consumo
            _channel.BasicConsume(
                queue: _settings.EnrollmentQueueName,
                autoAck: false,
                consumer: enrollmentConsumer
            );

            _channel.BasicConsume(
                queue: _settings.UnenrollmentQueueName,
                autoAck: false,
                consumer: unenrollmentConsumer
            );

            _logger.LogInformation("El consumidor de NotificationService está escuchando las colas de RabbitMQ");

            // Mantener el servicio corriendo
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("El servicio de RabbitMQ NotificationService fue cancelado");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio consumidor de RabbitMQ NotificationService");
        }
    }

    private async Task ProcessEnrollmentMessage(BasicDeliverEventArgs ea)
    {
        try
        {
            var messageBody = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(messageBody);

            var notificationMessage = JsonSerializer.Deserialize<NotificationRequest>(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (notificationMessage != null)
            {
                _logger.LogInformation("Procesando notificación de matrícula para {StudentEmail}", notificationMessage.StudentEmail);

                // Crear un scope para usar servicios scoped
                using var scope = _serviceScopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Enviar email de confirmación de matrícula
                await emailService.SendEnrollmentConfirmationAsync(
                    notificationMessage.StudentEmail,
                    notificationMessage.StudentName,
                    notificationMessage.CareerName,
                    notificationMessage.EnrollmentDate
                );

                _logger.LogInformation("Notificación de matrícula enviada a {StudentEmail}", notificationMessage.StudentEmail);
            }

            // Confirmar que el mensaje fue procesado
            _channel?.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en la notificación de matrícula");

            // Rechazar el mensaje y no requeued (evitar loops infinitos)
            _channel?.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    private async Task ProcessUnenrollmentMessage(BasicDeliverEventArgs ea)
    {
        try
        {
            var messageBody = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(messageBody);

            var notificationMessage = JsonSerializer.Deserialize<NotificationRequest>(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (notificationMessage != null)
            {
                _logger.LogInformation("Procesando notificación de desmatrícula para {StudentEmail}", notificationMessage.StudentEmail);

                // Crear un scope para usar servicios scoped
                using var scope = _serviceScopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Enviar email de confirmación de desmatrícula
                await emailService.SendEnrollmentCancellationAsync(
                    notificationMessage.StudentEmail,
                    notificationMessage.StudentName,
                    notificationMessage.CareerName,
                    notificationMessage.EnrollmentDate
                );

                _logger.LogInformation("Notificación de desmatrícula enviada a {StudentEmail}", notificationMessage.StudentEmail);
            }

            // Confirmar que el mensaje fue procesado
            _channel?.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en la notificación de desmatrícula");

            // Rechazar el mensaje y no requeued
            _channel?.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}