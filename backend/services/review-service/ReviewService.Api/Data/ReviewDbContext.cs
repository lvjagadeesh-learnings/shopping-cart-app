using Microsoft.EntityFrameworkCore;
using ReviewService.Api.Domain;

namespace ReviewService.Api.Data;

public sealed class ReviewDbContext(DbContextOptions<ReviewDbContext> options) : DbContext(options)
{
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("review");

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
            entity.Property(r => r.Comment).HasMaxLength(2000);
        });
    }
}
