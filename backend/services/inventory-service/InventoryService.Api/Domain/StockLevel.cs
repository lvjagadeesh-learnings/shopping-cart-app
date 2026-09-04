namespace InventoryService.Api.Domain;

/// <summary>Authoritative stock ledger for a single product. One row per product.</summary>
public sealed class StockLevel
{
    public required Guid ProductId { get; init; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Stock that can still be reserved right now.</summary>
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;
}
