using Microsoft.EntityFrameworkCore;
using RecommendationService.Api.Domain;

namespace RecommendationService.Api.Data;

public sealed class RecommendationDbContext(DbContextOptions<RecommendationDbContext> options) : DbContext(options)
{
    public DbSet<ProductView> ProductViews => Set<ProductView>();
    public DbSet<ProductPurchase> ProductPurchases => Set<ProductPurchase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("recommendation");

        modelBuilder.Entity<ProductView>(entity =>
        {
            entity.HasIndex(v => v.ProductId);
            entity.HasIndex(v => v.UserId);
        });

        modelBuilder.Entity<ProductPurchase>(entity =>
        {
            entity.HasIndex(p => p.ProductId);
            entity.HasIndex(p => p.UserId);
        });
    }
}
