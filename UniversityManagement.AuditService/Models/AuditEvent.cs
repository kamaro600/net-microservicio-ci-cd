namespace UniversityManagement.AuditService.Models;

/// <summary>
/// Modelo para eventos de auditoría
/// </summary>
public class AuditEvent
{
    public string EventType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string OldValues { get; set; } = string.Empty;
    public string NewValues { get; set; } = string.Empty;
    public string AdditionalData { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta del servicio de auditoría
/// </summary>
public class AuditResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}