using InventoryService.Api.Clients;
using InventoryService.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Data;

/// <summary>
/// Development-only bootstrap: gives every product Catalog Service currently knows about an
/// initial stock ledger row, so reserve/commit/release has something to operate against.
/// Idempotent — does nothing if any stock levels already exist.
/// </summary>
public static class InventoryDataSeeder
{
    public static async Task SeedAsync(InventoryDbContext db, ICatalogServiceClient catalogClient, CancellationToken ct)
    {
        if (await db.StockLevels.AnyAsync(ct))
        {
            return;
        }

        var productIds = await catalogClient.ListProductIdsAsync(ct);
        if (productIds.Count == 0)
        {
            return;
        }

        var rng = Random.Shared;
        foreach (var productId in productIds)
        {
            db.StockLevels.Add(new StockLevel
            {
                ProductId = productId,
                QuantityOnHand = rng.Next(15, 500)
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
