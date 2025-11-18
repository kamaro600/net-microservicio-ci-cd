namespace UniversityManagement.AuditService.Entities;

/// <summary>
/// Entidad para auditoría de eventos del sistema
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string OldValues { get; set; } = string.Empty;
    public string NewValues { get; set; } = string.Empty;
    public string AdditionalData { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}