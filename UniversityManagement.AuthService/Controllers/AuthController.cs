using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.AuthService.DTOs;
using UniversityManagement.AuthService.Services;

namespace UniversityManagement.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Autenticar usuario y generar JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            _logger.LogInformation("Usuario {Username} autenticado", request.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error de autenticacion para: {Username}", request.Username);
            return StatusCode(500, new AuthResponse 
            { 
                Success = false, 
                Message = "Internal server error durante autenticacion" 
            });
        }
    }

    /// <summary>
    /// Registrar nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            _logger.LogInformation("Usuario {Username} registrado", request.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro: {Username}", request.Username);
            return StatusCode(500, new AuthResponse 
            { 
                Success = false, 
                Message = "Internal server error durante registro" 
            });
        }
    }

    /// <summary>
    /// Validar token JWT
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<TokenValidationResponse>> ValidateToken([FromBody] TokenValidationRequest request)
    {
        try
        {
            var response = await _authService.ValidateTokenAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el token");
            return StatusCode(500, new TokenValidationResponse 
            { 
                IsValid = false, 
                Message = "Internal server error al validar token" 
            });
        }
    }

    /// <summary>
    /// Obtener información del usuario actual
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest(new { Message = "Invalid token claims" });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "Usuario no encontrado" });
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error alobtener informacion de usuario");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check del servicio de autenticación
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        try
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "AuthService",
                Database = "Connected",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check fallo");
            
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Service = "AuthService",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}