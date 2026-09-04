namespace OrderService.Api.Domain;

/// <summary>Append-only audit trail of every status transition an order goes through.</summary>
public sealed class OrderStatusHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrderId { get; init; }
    public required OrderStatus Status { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
