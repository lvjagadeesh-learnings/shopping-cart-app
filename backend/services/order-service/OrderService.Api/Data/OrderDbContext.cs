using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain;

namespace OrderService.Api.Data;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("order");

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.UserId);
            entity.Property(o => o.TotalAmount).HasColumnType("numeric(12,2)");
            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(i => new { i.OrderId, i.ProductId }).IsUnique();
            entity.Property(i => i.UnitPrice).HasColumnType("numeric(12,2)");
            entity.Property(i => i.ProductName).HasMaxLength(300).IsRequired();
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasIndex(h => h.OrderId);
        });
    }
}
