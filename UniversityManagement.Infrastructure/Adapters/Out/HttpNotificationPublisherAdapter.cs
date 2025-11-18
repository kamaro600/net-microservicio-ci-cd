using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniversityManagement.Application.DTOs.Messages;
using UniversityManagement.Application.Ports.Out;

namespace UniversityManagement.Infrastructure.Adapters.Out;

/// <summary>
/// Adaptador HTTP para comunicación con NotificationService
/// </summary>
public class HttpNotificationPublisherAdapter : IMessagePublisherPort
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpNotificationPublisherAdapter> _logger;
    private readonly string _notificationServiceUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpNotificationPublisherAdapter(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<HttpNotificationPublisherAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _notificationServiceUrl = configuration["MicroservicesUrls:NotificationService"] ?? "https://localhost:5065";
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task PublishEnrollmentNotificationAsync(EnrollmentNotificationMessage message)
    {
        try
        {
            _logger.LogInformation("Enviando notificación de matrícula vía HTTP para {StudentEmail}", message.StudentEmail);

            var notificationRequest = new
            {
                studentEmail = message.StudentEmail,
                studentName = message.StudentName,
                studentDni = message.StudentDni,
                careerName = message.CareerName,
                facultyName = message.FacultyName,
                enrollmentDate = message.EnrollmentDate,
                notificationType = "Enrollment",
                createdAt = message.CreatedAt,
                messageId = message.MessageId
            };

            var jsonContent = JsonSerializer.Serialize(notificationRequest, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_notificationServiceUrl}/api/notifications/enrollment", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notificación de matrícula enviada exitosamente para {StudentEmail}", message.StudentEmail);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al enviar notificación de matrícula para {StudentEmail}: {StatusCode} - {Error}", 
                    message.StudentEmail, response.StatusCode, errorContent);
                throw new HttpRequestException($"Error al enviar notificación: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falla al enviar notificación de matrícula para {StudentEmail}", message.StudentEmail);
            throw;
        }
    }

    public async Task PublishUnenrollmentNotificationAsync(EnrollmentNotificationMessage message)
    {
        try
        {
            _logger.LogInformation("Enviando notificación de desmatrícula vía HTTP para {StudentEmail}", message.StudentEmail);

            var notificationRequest = new
            {
                studentEmail = message.StudentEmail,
                studentName = message.StudentName,
                studentDni = message.StudentDni,
                careerName = message.CareerName,
                facultyName = message.FacultyName,
                enrollmentDate = message.EnrollmentDate,
                notificationType = "Unenrollment",
                createdAt = message.CreatedAt,
                messageId = message.MessageId
            };

            var jsonContent = JsonSerializer.Serialize(notificationRequest, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_notificationServiceUrl}/api/notifications/unenrollment", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notificación de desmatrícula enviada exitosamente para {StudentEmail}", message.StudentEmail);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al enviar notificación de desmatrícula para {StudentEmail}: {StatusCode} - {Error}", 
                    message.StudentEmail, response.StatusCode, errorContent);
                throw new HttpRequestException($"Error al enviar notificación: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falla al enviar notificación de desmatrícula para {StudentEmail}", message.StudentEmail);
            throw;
        }
    }
}