namespace UniversityManagement.NotificationService.Models;

/// <summary>
/// Modelo para solicitudes de notificación por email
/// </summary>
public class NotificationRequest
{
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentDni { get; set; } = string.Empty;
    public string CareerName { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public string NotificationType { get; set; } = string.Empty; // "Enrollment" o "Unenrollment"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Respuesta del servicio de notificación
/// </summary>
public class NotificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}