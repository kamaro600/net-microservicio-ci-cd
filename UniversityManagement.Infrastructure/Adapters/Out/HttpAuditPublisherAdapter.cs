using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniversityManagement.Application.DTOs.Messages;
using UniversityManagement.Application.Ports.Out;

namespace UniversityManagement.Infrastructure.Adapters.Out;

/// <summary>
/// Adaptador HTTP para comunicación con AuditService
/// </summary>
public class HttpAuditPublisherAdapter : IAuditPublisherPort
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpAuditPublisherAdapter> _logger;
    private readonly string _auditServiceUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpAuditPublisherAdapter(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<HttpAuditPublisherAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _auditServiceUrl = configuration["MicroservicesUrls:AuditService"] ?? "https://localhost:5066";
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task PublishAuditEventAsync(AuditEventMessage auditEvent)
    {
        try
        {
            _logger.LogInformation("Enviando evento de auditoría vía HTTP: {EventType} - {Action} - {EntityId}", 
                auditEvent.EventType, auditEvent.Action, auditEvent.EntityId);

            var jsonContent = JsonSerializer.Serialize(auditEvent, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_auditServiceUrl}/api/audit/events", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Evento de auditoría enviado exitosamente: {EventType} - {EntityId}", 
                    auditEvent.EventType, auditEvent.EntityId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al enviar evento de auditoría {EventType} - {EntityId}: {StatusCode} - {Error}", 
                    auditEvent.EventType, auditEvent.EntityId, response.StatusCode, errorContent);
                throw new HttpRequestException($"Error al enviar evento de auditoría: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falla al enviar evento de auditoría {EventType} - {EntityId}", 
                auditEvent.EventType, auditEvent.EntityId);
            throw;
        }
    }

    public async Task PublishEnrollmentAuditAsync(string studentId, string careerId, string action, string? additionalData = null)
    {
        var auditEvent = new AuditEventMessage
        {
            EventType = "Enrollment",
            EntityName = "StudentCareer",
            EntityId = $"{studentId}-{careerId}",
            Action = action,
            UserId = "System",
            UserName = "System User",
            Timestamp = DateTime.UtcNow,
            AdditionalData = additionalData ?? string.Empty
        };

        await PublishAuditEventAsync(auditEvent);
    }

    public async Task PublishStudentAuditAsync(string studentId, string action, string? oldValues = null, string? newValues = null)
    {
        var auditEvent = new AuditEventMessage
        {
            EventType = "Student",
            EntityName = "Student",
            EntityId = studentId,
            Action = action,
            UserId = "System",
            UserName = "System User",
            Timestamp = DateTime.UtcNow,
            OldValues = oldValues ?? string.Empty,
            NewValues = newValues ?? string.Empty
        };

        await PublishAuditEventAsync(auditEvent);
    }

    public async Task PublishBulkAuditEventsAsync(IEnumerable<AuditEventMessage> auditEvents)
    {
        try
        {
            _logger.LogInformation("Enviando {Count} eventos de auditoría en lote vía HTTP", auditEvents.Count());

            var jsonContent = JsonSerializer.Serialize(auditEvents, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_auditServiceUrl}/api/audit/events/bulk", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Eventos de auditoría en lote enviados exitosamente: {Count} eventos", auditEvents.Count());
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al enviar eventos de auditoría en lote: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Error al enviar eventos de auditoría en lote: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falla al enviar eventos de auditoría en lote: {Count} eventos", auditEvents.Count());
            throw;
        }
    }
}