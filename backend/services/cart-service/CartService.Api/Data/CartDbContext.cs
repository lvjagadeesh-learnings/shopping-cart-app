using CartService.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CartService.Api.Data;

public sealed class CartDbContext(DbContextOptions<CartDbContext> options) : DbContext(options)
{
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("cart");

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(c => c.UserId).IsUnique();
            entity.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();
            entity.Property(i => i.UnitPrice).HasColumnType("numeric(12,2)");
            entity.Property(i => i.ProductName).HasMaxLength(300).IsRequired();
        });
    }
}
