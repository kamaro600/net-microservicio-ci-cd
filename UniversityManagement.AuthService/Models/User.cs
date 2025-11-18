using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.AuthService.Models;

/// <summary>
/// Entidad de usuario para autenticación
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    // Navegación a roles
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// Entidad de rol
/// </summary>
public class Role
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navegación a usuarios
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// Relación muchos a muchos entre usuarios y roles
/// </summary>
public class UserRole
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}