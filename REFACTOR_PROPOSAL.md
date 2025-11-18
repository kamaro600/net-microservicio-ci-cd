# 🔧 REFACTORIZACIÓN DEL AUTH SERVICE

## Problemas Identificados

### ❌ Arquitectura Actual (INCORRECTA)
```
Controller -> Service (SQL directo + EF + Lógica de negocio + Validaciones)
```

### ✅ Arquitectura Propuesta (CORRECTA)
```
Controller -> Service -> Repository -> DbContext
           -> Domain Services
           -> DTOs/Mappers
```

## Estructura Propuesta

### 1. Repositories (Acceso a Datos)
```csharp
// Repositories/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<bool> UpdateLastLoginAsync(int userId, DateTime lastLogin);
    Task<bool> ExistsAsync(string username, string email);
    Task<List<string>> GetUserRolesAsync(int userId);
}

// Repositories/UserRepository.cs
public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;
    
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }
    
    // Implementar otros métodos usando solo EF Core
}
```

### 2. Domain Services (Lógica de Negocio)
```csharp
// Domain/Services/IPasswordService.cs
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    bool IsValidPassword(string password);
}

// Domain/Services/IAuthDomainService.cs
public interface IAuthDomainService
{
    bool IsValidLoginAttempt(User user, string password);
    void ValidateRegistrationData(RegisterRequest request);
}
```

### 3. Application Services (Casos de Uso)
```csharp
// Services/AuthenticationService.cs (REFACTORIZADO)
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IAuthDomainService _authDomainService;
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // 1. Validar entrada
        if (string.IsNullOrEmpty(request.Username))
            return AuthResponse.Failure("Username es requerido");
            
        // 2. Obtener usuario (Repository)
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
            return AuthResponse.Failure("Credenciales inválidas");
            
        // 3. Validar credenciales (Domain Service)
        if (!_authDomainService.IsValidLoginAttempt(user, request.Password))
            return AuthResponse.Failure("Credenciales inválidas");
            
        // 4. Obtener roles (Repository)
        var roles = await _userRepository.GetUserRolesAsync(user.Id);
        
        // 5. Generar token (JWT Service)
        var token = _jwtService.GenerateToken(user, roles);
        
        // 6. Actualizar último login (Repository)
        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);
        
        // 7. Mapear respuesta
        return AuthResponse.Success(token, user.ToUserInfo(roles));
    }
}
```

### 4. DTOs y Mappers
```csharp
// DTOs/AuthResponse.cs
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? Token { get; set; }
    public UserInfo? User { get; set; }
    
    public static AuthResponse Success(string token, UserInfo user) => new()
    {
        Success = true,
        Token = token,
        User = user,
        Message = "Login exitoso"
    };
    
    public static AuthResponse Failure(string message) => new()
    {
        Success = false,
        Message = message
    };
}

// Mappers/UserMapper.cs
public static class UserMapper
{
    public static UserInfo ToUserInfo(this User user, List<string> roles) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Roles = roles
    };
}
```

## Beneficios de la Refactorización

### ✅ Separación de Responsabilidades
- **Controller**: Solo manejo HTTP
- **Service**: Lógica de aplicación/casos de uso
- **Repository**: Acceso a datos
- **Domain Services**: Reglas de negocio

### ✅ Testabilidad Mejorada
```csharp
// Fácil de testear cada componente por separado
var mockUserRepo = new Mock<IUserRepository>();
var mockJwtService = new Mock<IJwtService>();
var authService = new AuthenticationService(mockUserRepo.Object, mockJwtService.Object);
```

### ✅ Mantenibilidad
- Cambios en BD solo afectan Repository
- Cambios en lógica solo afectan Services
- Fácil agregar nuevas funcionalidades

### ✅ Consistencia
- Solo EF Core para acceso a datos
- No mezcla de tecnologías
- Patrones consistentes

## Plan de Implementación

1. **Crear Repositories** (IUserRepository, IRoleRepository)
2. **Crear Domain Services** (IPasswordService, IAuthDomainService)  
3. **Refactorizar AuthenticationService** (usar inyección de dependencias)
4. **Crear Mappers** (User -> UserInfo)
5. **Agregar Tests Unitarios**
6. **Eliminar SQL directo**
7. **Validar funcionalidad**

## Prioridad: ALTA 🚨
Este refactor debería hacerse antes de producción para mantener código limpio y mantenible.