using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Domain;

namespace NotificationService.Api.Data;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notification");

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasIndex(n => n.UserId);
            entity.Property(n => n.EventType).HasMaxLength(64);
            entity.Property(n => n.Title).HasMaxLength(200);
            entity.Property(n => n.Message).HasMaxLength(1000);
        });
    }
}
