using Microsoft.EntityFrameworkCore;
using UniversityManagement.AuditService.Entities;

namespace UniversityManagement.AuditService.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("audit_logs");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(a => a.EventType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.EntityId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.OldValues)
                .HasColumnType("text");

            builder.Property(a => a.NewValues)
                .HasColumnType("text");

            builder.Property(a => a.UserId)
                .HasMaxLength(36);

            builder.Property(a => a.UserName)
                .HasMaxLength(100);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45);

            builder.Property(a => a.UserAgent)
                .HasMaxLength(500);

            builder.Property(a => a.AdditionalData)
                .HasColumnType("text");

            builder.Property(a => a.Timestamp)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            builder.HasIndex(a => a.EventType);
            builder.HasIndex(a => a.EntityName);
            builder.HasIndex(a => a.EntityId);
            builder.HasIndex(a => a.Action);
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.Timestamp);

            builder.HasIndex(a => new { a.EventType, a.EntityId, a.Timestamp });
        });
    }
}