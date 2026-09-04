namespace InventoryService.Api.Domain;

public enum ReservationStatus
{
    Reserved,
    Committed,
    Released
}

/// <summary>
/// A hold placed against a product's stock for a specific order, created during checkout.
/// Committed when the order's payment succeeds; released if payment fails or the order is cancelled.
/// </summary>
public sealed class StockReservation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrderId { get; init; }
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Reserved;
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
