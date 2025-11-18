using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.NotificationService.Models;
using UniversityManagement.NotificationService.Services;

namespace UniversityManagement.NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly RabbitMQConnectionService _rabbitMQService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        RabbitMQConnectionService rabbitMQService, 
        ILogger<NotificationsController> logger)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    /// <summary>
    /// Envía una notificación de matrícula
    /// </summary>
    [HttpPost("enrollment")]
    public async Task<ActionResult<NotificationResponse>> SendEnrollmentNotification(
        [FromBody] NotificationRequest request)
    {
        try
        {
            request.NotificationType = "Enrollment";
            await _rabbitMQService.PublishNotificationAsync(request);

            var response = new NotificationResponse
            {
                Success = true,
                Message = "Notificacion de matricula enviada",
                MessageId = request.MessageId
            };

            _logger.LogInformation("Notificacion matricula estudiante: {StudentEmail}", request.StudentEmail);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo en envio de notificacion matricula: {StudentEmail}", request.StudentEmail);
            
            var errorResponse = new NotificationResponse
            {
                Success = false,
                Message = $"Fallo: {ex.Message}",
                MessageId = request.MessageId
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Envía una notificación de desmatrícula
    /// </summary>
    [HttpPost("unenrollment")]
    public async Task<ActionResult<NotificationResponse>> SendUnenrollmentNotification(
        [FromBody] NotificationRequest request)
    {
        try
        {
            request.NotificationType = "Unenrollment";
            await _rabbitMQService.PublishNotificationAsync(request);

            var response = new NotificationResponse
            {
                Success = true,
                Message = "Notificacion de Desmatricula exitosa",
                MessageId = request.MessageId
            };

            _logger.LogInformation("Notificacion desmatricula estudiante: {StudentEmail}", request.StudentEmail);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo: {StudentEmail}", request.StudentEmail);
            
            var errorResponse = new NotificationResponse
            {
                Success = false,
                Message = $"Fallo: {ex.Message}",
                MessageId = request.MessageId
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Health check del servicio de notificaciones
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        try
        {
            var channel = _rabbitMQService.GetChannel();
            
            return Ok(new
            {
                Status = "Healthy",
                Service = "NotificationService",
                RabbitMQ = "Connected",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check fallo");
            
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Service = "NotificationService",
                RabbitMQ = "Disconnected",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}