using BuildingBlocks.Common;
using InventoryService.Api.Contracts;
using InventoryService.Api.Data;
using InventoryService.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Services;

public sealed class InventoryManager(InventoryDbContext db)
{
    /// <summary>
    /// Reserves stock for every item on an order, all-or-nothing. Idempotent: if reservations
    /// already exist for this order (e.g. a retried checkout request), they're returned as-is
    /// instead of reserving again.
    /// </summary>
    public async Task<IReadOnlyList<StockReservationResponse>> ReserveAsync(ReserveStockRequest request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
        {
            throw new ValidationApiException("At least one item is required.");
        }

        if (request.Items.Any(i => i.Quantity <= 0))
        {
            throw new ValidationApiException("Quantity must be greater than zero.");
        }

        var existing = await db.StockReservations.Where(r => r.OrderId == request.OrderId).ToListAsync(ct);
        if (existing.Count > 0)
        {
            return existing.Select(r => r.ToResponse()).ToList();
        }

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var stockLevels = await db.StockLevels
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId, ct);

        foreach (var item in request.Items)
        {
            if (!stockLevels.TryGetValue(item.ProductId, out var stock) || stock.QuantityAvailable < item.Quantity)
            {
                throw new ConflictException($"Insufficient stock for product '{item.ProductId}'.");
            }
        }

        var reservations = new List<StockReservation>();
        foreach (var item in request.Items)
        {
            var stock = stockLevels[item.ProductId];
            stock.QuantityReserved += item.Quantity;
            stock.UpdatedAtUtc = DateTimeOffset.UtcNow;

            var reservation = new StockReservation
            {
                OrderId = request.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            };
            reservations.Add(reservation);
            db.StockReservations.Add(reservation);
        }

        await db.SaveChangesAsync(ct);
        return reservations.Select(r => r.ToResponse()).ToList();
    }

    /// <summary>Confirms an order's reservations as a completed sale: stock leaves the warehouse permanently.</summary>
    public async Task<IReadOnlyList<StockReservationResponse>> CommitAsync(Guid orderId, CancellationToken ct)
    {
        var reservations = await db.StockReservations.Where(r => r.OrderId == orderId).ToListAsync(ct);
        if (reservations.Count == 0)
        {
            throw new NotFoundException($"No reservations found for order '{orderId}'.");
        }

        var pending = reservations.Where(r => r.Status == ReservationStatus.Reserved).ToList();
        if (pending.Count > 0)
        {
            var productIds = pending.Select(r => r.ProductId).Distinct().ToList();
            var stockLevels = await db.StockLevels
                .Where(s => productIds.Contains(s.ProductId))
                .ToDictionaryAsync(s => s.ProductId, ct);

            foreach (var reservation in pending)
            {
                var stock = stockLevels[reservation.ProductId];
                stock.QuantityOnHand -= reservation.Quantity;
                stock.QuantityReserved -= reservation.Quantity;
                stock.UpdatedAtUtc = DateTimeOffset.UtcNow;
                reservation.Status = ReservationStatus.Committed;
                reservation.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync(ct);
        }

        return reservations.Select(r => r.ToResponse()).ToList();
    }

    /// <summary>Releases an order's outstanding reservations (payment failed / order cancelled): the hold is dropped, stock is untouched.</summary>
    public async Task<IReadOnlyList<StockReservationResponse>> ReleaseAsync(Guid orderId, CancellationToken ct)
    {
        var reservations = await db.StockReservations.Where(r => r.OrderId == orderId).ToListAsync(ct);
        if (reservations.Count == 0)
        {
            throw new NotFoundException($"No reservations found for order '{orderId}'.");
        }

        var pending = reservations.Where(r => r.Status == ReservationStatus.Reserved).ToList();
        if (pending.Count > 0)
        {
            var productIds = pending.Select(r => r.ProductId).Distinct().ToList();
            var stockLevels = await db.StockLevels
                .Where(s => productIds.Contains(s.ProductId))
                .ToDictionaryAsync(s => s.ProductId, ct);

            foreach (var reservation in pending)
            {
                var stock = stockLevels[reservation.ProductId];
                stock.QuantityReserved -= reservation.Quantity;
                stock.UpdatedAtUtc = DateTimeOffset.UtcNow;
                reservation.Status = ReservationStatus.Released;
                reservation.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync(ct);
        }

        return reservations.Select(r => r.ToResponse()).ToList();
    }

    public async Task<StockLevelResponse> GetStockLevelAsync(Guid productId, CancellationToken ct)
    {
        var stock = await db.StockLevels.FindAsync([productId], ct)
            ?? throw new NotFoundException($"No stock level found for product '{productId}'.");

        return stock.ToResponse();
    }

    /// <summary>Sets the on-hand quantity for a product (e.g. restock), creating the ledger row if it doesn't exist yet.</summary>
    public async Task<StockLevelResponse> AdjustStockAsync(Guid productId, AdjustStockRequest request, CancellationToken ct)
    {
        if (request.QuantityOnHand < 0)
        {
            throw new ValidationApiException("Quantity on hand cannot be negative.");
        }

        var stock = await db.StockLevels.FindAsync([productId], ct);
        if (stock is null)
        {
            stock = new StockLevel { ProductId = productId, QuantityOnHand = request.QuantityOnHand };
            db.StockLevels.Add(stock);
        }
        else
        {
            stock.QuantityOnHand = request.QuantityOnHand;
            stock.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return stock.ToResponse();
    }
}
