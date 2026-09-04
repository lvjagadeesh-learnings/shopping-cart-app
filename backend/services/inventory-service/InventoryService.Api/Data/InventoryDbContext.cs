using InventoryService.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");

        modelBuilder.Entity<StockLevel>(entity =>
        {
            entity.HasKey(s => s.ProductId);
        });

        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.HasIndex(r => r.OrderId);
            entity.HasIndex(r => new { r.OrderId, r.ProductId });
        });
    }
}
