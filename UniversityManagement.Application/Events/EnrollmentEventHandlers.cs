using UniversityManagement.Domain.Events;
using UniversityManagement.Application.Ports.Out;
using UniversityManagement.Application.DTOs.Messages;
using Microsoft.Extensions.Logging;

namespace UniversityManagement.Application.Events;

public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

public class StudentEnrolledEventHandler : IDomainEventHandler<StudentEnrolledEvent>
{
    private readonly IMessagePublisherPort _messagePublisher;
    private readonly IAuditPublisherPort _auditPublisher;
    private readonly ILogger<StudentEnrolledEventHandler> _logger;

    public StudentEnrolledEventHandler(
        IMessagePublisherPort messagePublisher,
        IAuditPublisherPort auditPublisher,
        ILogger<StudentEnrolledEventHandler> logger)
    {
        _messagePublisher = messagePublisher;
        _auditPublisher = auditPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(StudentEnrolledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Enviar notificación de matrícula
            var notificationMessage = new EnrollmentNotificationMessage
            {
                StudentEmail = domainEvent.StudentEmail,
                StudentName = domainEvent.StudentName,
                StudentDni = domainEvent.StudentId.ToString(), // Convirtiendo int a string
                CareerName = domainEvent.CareerName,
                FacultyName = "Unknown Faculty", //Obtener del dominio
                EnrollmentDate = domainEvent.EnrollmentDate,
                NotificationType = "Enrollment"
            };
            
            await _messagePublisher.PublishEnrollmentNotificationAsync(notificationMessage);

            // Registrar evento de auditoría
            var auditMessage = new AuditEventMessage
            {
                EventType = "Enrollment",
                EntityName = "StudentCareer",
                EntityId = $"{domainEvent.StudentId}-{domainEvent.CareerId}",
                Action = "Created",
                UserId = domainEvent.EnrolledBy ?? "system",
                UserName = domainEvent.EnrolledBy ?? "System User",
                Timestamp = domainEvent.OccurredOn,
                NewValues = $"StudentId: {domainEvent.StudentId}, CareerId: {domainEvent.CareerId}, StudentName: {domainEvent.StudentName}, CareerName: {domainEvent.CareerName}, EnrollmentDate: {domainEvent.EnrollmentDate}",
                AdditionalData = $"StudentEmail: {domainEvent.StudentEmail}"
            };
            
            await _auditPublisher.PublishAuditEventAsync(auditMessage);

            _logger.LogInformation("Successfully processed StudentEnrolled event for Student: {StudentId}, Career: {CareerId}", 
                domainEvent.StudentId, domainEvent.CareerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process StudentEnrolled event for Student: {StudentId}, Career: {CareerId}", 
                domainEvent.StudentId, domainEvent.CareerId);
            throw;
        }
    }
}

public class StudentUnenrolledEventHandler : IDomainEventHandler<StudentUnenrolledEvent>
{
    private readonly IMessagePublisherPort _messagePublisher;
    private readonly IAuditPublisherPort _auditPublisher;
    private readonly ILogger<StudentUnenrolledEventHandler> _logger;

    public StudentUnenrolledEventHandler(
        IMessagePublisherPort messagePublisher,
        IAuditPublisherPort auditPublisher,
        ILogger<StudentUnenrolledEventHandler> logger)
    {
        _messagePublisher = messagePublisher;
        _auditPublisher = auditPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(StudentUnenrolledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Enviar notificación de desmatrícula
            var notificationMessage = new EnrollmentNotificationMessage
            {
                StudentEmail = domainEvent.StudentEmail,
                StudentName = domainEvent.StudentName,
                StudentDni = domainEvent.StudentId.ToString(), // Convirtiendo int a string
                CareerName = domainEvent.CareerName,
                FacultyName = "Unknown Faculty", // Obtener del dominio
                EnrollmentDate = domainEvent.UnenrollmentDate,
                NotificationType = "Unenrollment"
            };
            
            await _messagePublisher.PublishUnenrollmentNotificationAsync(notificationMessage);

            // Registrar evento de auditoría
            var auditMessage = new AuditEventMessage
            {
                EventType = "Unenrollment",
                EntityName = "StudentCareer",
                EntityId = $"{domainEvent.StudentId}-{domainEvent.CareerId}",
                Action = "Deleted",
                UserId = domainEvent.UnenrolledBy ?? "system",
                UserName = domainEvent.UnenrolledBy ?? "System User",
                Timestamp = domainEvent.OccurredOn,
                OldValues = $"StudentId: {domainEvent.StudentId}, CareerId: {domainEvent.CareerId}, StudentName: {domainEvent.StudentName}, CareerName: {domainEvent.CareerName}",
                AdditionalData = $"StudentEmail: {domainEvent.StudentEmail}, UnenrollmentDate: {domainEvent.UnenrollmentDate}"
            };
            
            await _auditPublisher.PublishAuditEventAsync(auditMessage);

            _logger.LogInformation("eventos para Matricula satisfactoria Estudiante: {StudentId}, Carrera: {CareerId}", 
                domainEvent.StudentId, domainEvent.CareerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo proceso de envio eventos para Matricula Estudiante: {StudentId}, Carrera: {CareerId}", 
                domainEvent.StudentId, domainEvent.CareerId);
            throw;
        }
    }
}