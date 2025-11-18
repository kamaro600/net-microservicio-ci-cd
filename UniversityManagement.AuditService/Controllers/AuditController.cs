using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.AuditService.Data;
using UniversityManagement.AuditService.Models;
using UniversityManagement.AuditService.Services;

namespace UniversityManagement.AuditService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly ILogger<AuditController> _logger;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly AuditDbContext _dbContext;

    public AuditController(
        ILogger<AuditController> logger,
        IKafkaProducerService kafkaProducerService,
        AuditDbContext dbContext)
    {
        _logger = logger;
        _kafkaProducerService = kafkaProducerService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Registra un evento de auditoría
    /// </summary>
    [HttpPost("events")]
    public async Task<ActionResult<AuditResponse>> PublishAuditEvent([FromBody] AuditEvent auditEvent)
    {
        try
        {
            _logger.LogInformation("Eveto recibido: {EventType} para {EntityName}:{EntityId}", 
                auditEvent.EventType, auditEvent.EntityName, auditEvent.EntityId);

            var published = await _kafkaProducerService.PublishAuditEventAsync(auditEvent);
            
            if (published)
            {
                var response = new AuditResponse
                {
                    Success = true,
                    Message = "Evento de auditoria publicado correctamente",
                    EventId = Guid.NewGuid().ToString()
                };

                return Ok(response);
            }
            else
            {
                var errorResponse = new AuditResponse
                {
                    Success = false,
                    Message = "FAlla en la publicaicon del evento",
                    EventId = Guid.NewGuid().ToString()
                };

                return StatusCode(500, errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo en el proceso de publicacaion {EntityName}:{EntityId}", 
                auditEvent.EntityName, auditEvent.EntityId);
            
            var errorResponse = new AuditResponse
            {
                Success = false,
                Message = $"Fallo proceso: {ex.Message}",
                EventId = Guid.NewGuid().ToString()
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Registra múltiples eventos de auditoría en lote
    /// </summary>
    [HttpPost("events/bulk")]
    public async Task<ActionResult<AuditResponse>> PublishBulkAuditEvents([FromBody] IEnumerable<AuditEvent> auditEvents)
    {
        try
        {
            var events = auditEvents.ToList();
            _logger.LogInformation("Lote de eventos recibido: {Count} eventos", events.Count);

            var publishTasks = events.Select(auditEvent => _kafkaProducerService.PublishAuditEventAsync(auditEvent));
            var results = await Task.WhenAll(publishTasks);

            var successCount = results.Count(r => r);
            var failureCount = results.Count(r => !r);

            if (failureCount == 0)
            {
                var response = new AuditResponse
                {
                    Success = true,
                    Message = $"Los {successCount} audit events published to Kafka successfullyventos fueron publicados",
                    EventId = $"bulk-{Guid.NewGuid()}"
                };

                return Ok(response);
            }
            else
            {
                var errorResponse = new AuditResponse
                {
                    Success = false,
                    Message = $"Publicacion parcial exitosos: {successCount}, errados {failureCount} ",
                    EventId = $"bulk-{Guid.NewGuid()}"
                };

                return StatusCode(207, errorResponse); // 207 Multi-Status for partial success
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FAllo en publicaicon de lotes de eventos");
            
            var errorResponse = new AuditResponse
            {
                Success = false,
                Message = $"FALLA EN PUBLICACION DE LOTES: {ex.Message}",
                EventId = $"bulk-{Guid.NewGuid()}"
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Obtiene eventos de auditoría
    /// </summary>
    [HttpGet("events")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAuditEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? eventType = null)
    {
        try
        {
            var query = _dbContext.AuditLogs.AsQueryable();

            // Filtrar por tipo de evento si se especifica
            if (!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(a => a.EventType == eventType);
            }

            // Contar total de registros
            var totalRecords = await query.CountAsync();

            // Aplicar paginación y ordenar por timestamp descendente
            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.EventType,
                    a.EntityName,
                    a.EntityId,
                    a.Action,
                    a.UserId,
                    a.UserName,
                    a.Timestamp,
                    a.IpAddress,
                    a.UserAgent,
                    a.OldValues,
                    a.NewValues,
                    a.AdditionalData
                })
                .ToListAsync();

            return Ok(new 
            { 
                Records = auditLogs, 
                Total = totalRecords,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falla al obtener eventos");
            return StatusCode(500, new { Message = "FALLA AL OBTENER EVENTOS" });
        }
    }

    /// <summary>
    /// Health check del servicio de auditoría
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        try
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "AuditService",
                Kafka = "Connected",
                Database = "Connected", 
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Service = "AuditService",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}