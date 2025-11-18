using Microsoft.EntityFrameworkCore;
using UniversityManagement.AuthService.Models;

namespace UniversityManagement.AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();           
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100);
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => new { e.UserId, e.RoleId });           
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            
            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);
            
            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

       // SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Roles por defecto
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Administrator with full access" },
            new Role { Id = 2, Name = "User", Description = "Regular user with limited access" },
            new Role { Id = 3, Name = "Student", Description = "Student role for enrollment operations" },
            new Role { Id = 4, Name = "Professor", Description = "Professor role for academic operations" }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@universidad.edu",
                PasswordHash = "$2a$11$8gE7mCq8St4jZCRJvg8poe3h0.Dq0rRQY8wy4F4vDd.3rRm.GCH5y", // Admin123!
                FirstName = "Administrator",
                LastName = "System",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Asignar rol admin al usuario admin
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = 1, RoleId = 1, AssignedAt = DateTime.UtcNow }
        );
    }
}