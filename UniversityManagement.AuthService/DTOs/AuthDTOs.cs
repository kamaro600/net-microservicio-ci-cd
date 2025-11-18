using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.AuthService.DTOs;

/// <summary>
/// Solicitud de login
/// </summary>
public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Solicitud de registro
/// </summary>
public class RegisterRequest
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta de autenticación exitosa
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = null!;
}

/// <summary>
/// Información básica del usuario
/// </summary>
public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Solicitud de verificación de token
/// </summary>
public class TokenValidationRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta de verificación de token
/// </summary>
public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfo? User { get; set; }
}