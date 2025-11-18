using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.AuthService.Data;
using UniversityManagement.AuthService.DTOs;
using UniversityManagement.AuthService.Models;
using Npgsql;

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
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        AuthDbContext dbContext,
        IJwtService jwtService,
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("AuthConnection");
            _logger.LogInformation("AuthService: Usuario: {Username}", request.Username);
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("AuthService: Conexion exitosa");
            
            using var dbCheckCommand = new NpgsqlCommand("SELECT current_database()", connection);
            var currentDb = await dbCheckCommand.ExecuteScalarAsync();            
            
            var userQuery = @"
                SELECT id, username, email, password_hash, first_name, last_name, is_active, created_at, last_login_at
                FROM users 
                WHERE username = @username AND is_active = true";
            
            using var userCommand = new NpgsqlCommand(userQuery, connection);
            userCommand.Parameters.AddWithValue("username", request.Username);
            _logger.LogInformation("AuthService: Ejecutando: {Query}", userQuery);
            User? user = null;
            using var reader = await userCommand.ExecuteReaderAsync();
            _logger.LogInformation("AuthService: Ejecutado");
            
            if (await reader.ReadAsync())
            {
                _logger.LogInformation("AuthService: Usuario encontrado");
                user = new User
                {
                    Id = reader.GetInt32(0), 
                    Username = reader.GetString(1), 
                    Email = reader.GetString(2), 
                    PasswordHash = reader.GetString(3), 
                    FirstName = reader.IsDBNull(4) ? "" : reader.GetString(4), 
                    LastName = reader.IsDBNull(5) ? "" : reader.GetString(5), 
                    IsActive = reader.GetBoolean(6), 
                    CreatedAt = reader.GetDateTime(7), 
                    LastLoginAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8) 
                };
            }
            
            if (user == null)
            {
                _logger.LogWarning("AuthService: Usuario no encontrado: {Username}", request.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Usuario o password invalido"
                };
            }
            
            _logger.LogInformation("AuthService: usuario cargo exitosamente: {UserId} - {Username}", user.Id, user.Username);

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Usuario o password invalido"
                };
            }

            reader.Close();
            
            var rolesQuery = @"
                SELECT r.name 
                FROM roles r
                INNER JOIN user_roles ur ON r.id = ur.role_id
                WHERE ur.user_id = @userId";
            
            using var rolesCommand = new NpgsqlCommand(rolesQuery, connection);
            rolesCommand.Parameters.AddWithValue("userId", user.Id);
            
            var roles = new List<string>();
            using var rolesReader = await rolesCommand.ExecuteReaderAsync();
            
            while (await rolesReader.ReadAsync())
            {
                roles.Add(rolesReader.GetString(0));
            }
            
            rolesReader.Close();

            // Actualizar último login
            var updateQuery = @"UPDATE users SET last_login_at = @lastLogin WHERE id = @userId";
            using var updateCommand = new NpgsqlCommand(updateQuery, connection);
            updateCommand.Parameters.AddWithValue("lastLogin", DateTime.UtcNow);
            updateCommand.Parameters.AddWithValue("userId", user.Id);
            await updateCommand.ExecuteNonQueryAsync();

            // Generar token
            var token = _jwtService.GenerateToken(user, roles);

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
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
                Message = "Un error en el login"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Verificar si el usuario ya existe
            var existingUser = await _dbContext.Users
                .FromSqlRaw(@"
                    SELECT u.id, u.username, u.email, u.password_hash, u.first_name, u.last_name, 
                           u.is_active, u.created_at, u.last_login_at
                    FROM users u 
                    WHERE u.username = {0} OR u.email = {1}", request.Username, request.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
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

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = 2, 
                AssignedAt = DateTime.UtcNow
            };

            _dbContext.UserRoles.Add(userRole);
            await _dbContext.SaveChangesAsync();


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
                Message = "Un error durante el registro"
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
            var connectionString = _configuration.GetConnectionString("AuthConnection");
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            var query = @"
                SELECT id, username, email, password_hash, first_name, last_name, is_active, created_at, last_login_at
                FROM users 
                WHERE id = @userId AND is_active = true";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("userId", userId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0), 
                    Username = reader.GetString(1), 
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3), 
                    FirstName = reader.IsDBNull(4) ? "" : reader.GetString(4), 
                    LastName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    IsActive = reader.GetBoolean(6), 
                    CreatedAt = reader.GetDateTime(7), 
                    LastLoginAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8) 
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
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