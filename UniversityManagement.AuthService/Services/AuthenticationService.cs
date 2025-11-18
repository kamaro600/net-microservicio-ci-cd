using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.AuthService.Data;
using UniversityManagement.AuthService.DTOs;
using UniversityManagement.AuthService.Models;

namespace UniversityManagement.AuthService.Services;

public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<TokenValidationResponse> ValidateTokenAsync(TokenValidationRequest request);
    Task<User?> GetUserByIdAsync(int userId);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly AuthDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        AuthDbContext dbContext,
        IJwtService jwtService,
        ILogger<AuthenticationService> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Intento de login para usuario: {Username}", request.Username);

            // Validar entrada básica
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Usuario y contraseña son requeridos"
                };
            }

            // Buscar usuario usando Entity Framework
            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado: {Username}", request.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Usuario o contraseña inválidos"
                };
            }

            // Verificar contraseña
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Contraseña inválida para usuario: {Username}", request.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Usuario o contraseña inválidos"
                };
            }

            // Obtener roles del usuario
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Actualizar último login
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Generar token
            var token = _jwtService.GenerateToken(user, roles);

            _logger.LogInformation("Usuario {Username} autenticado correctamente", user.Username);

            return new AuthResponse
            {
                Success = true,
                Message = "Login exitoso",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(120),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login: {Username}", request.Username);
            
            return new AuthResponse
            {
                Success = false,
                Message = "Error interno del servidor"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Intento de registro para usuario: {Username}", request.Username);

            // Validar datos de entrada
            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Todos los campos son requeridos"
                };
            }

            // Verificar si el usuario ya existe usando EF
            var existingUser = await _dbContext.Users
                .AnyAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Usuario o email ya existen"
                };
            }

            // Crear nuevo usuario
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Asignar rol por defecto "User"
            var defaultRole = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == "User");
            
            var roleId = defaultRole?.Id ?? 2; // Fallback a ID 2

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            };

            _dbContext.UserRoles.Add(userRole);
            await _dbContext.SaveChangesAsync();

            // Generar token
            var roles = new List<string> { "User" };
            var token = _jwtService.GenerateToken(user, roles);

            _logger.LogInformation("Usuario {Username} registrado correctamente", user.Username);

            return new AuthResponse
            {
                Success = true,
                Message = "Registro exitoso",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante registro: {Username}", request.Username);
            
            return new AuthResponse
            {
                Success = false,
                Message = "Error interno del servidor durante el registro"
            };
        }
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(TokenValidationRequest request)
    {
        try
        {
            var principal = _jwtService.ValidateToken(request.Token);
            
            if (principal == null)
            {
                return new TokenValidationResponse
                {
                    IsValid = false,
                    Message = "Token invalido o expirado "
                };
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new TokenValidationResponse
                {
                    IsValid = false,
                    Message = "Token claims invalidos"
                };
            }

            var user = await GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new TokenValidationResponse
                {
                    IsValid = false,
                    Message = "Usuario no encontrado o inactivo"
                };
            }

            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return new TokenValidationResponse
            {
                IsValid = true,
                Message = "Token is valid",
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la validaicon de token");
            
            return new TokenValidationResponse
            {
                IsValid = false,
                Message = "Validaicon de token fallo"
            };
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            return await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo usuario por ID: {UserId}", userId);
            return null;
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        string dd=BCrypt.Net.BCrypt.HashPassword(password);

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}