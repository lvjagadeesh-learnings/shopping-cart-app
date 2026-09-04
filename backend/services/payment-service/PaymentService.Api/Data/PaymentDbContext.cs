using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Domain;

namespace PaymentService.Api.Data;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payment");

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.OrderId).IsUnique();
            entity.Property(p => p.Amount).HasColumnType("numeric(12,2)");
            entity.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasIndex(t => t.PaymentId);
        });
    }
}
