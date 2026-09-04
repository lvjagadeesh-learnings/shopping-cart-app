namespace OrderService.Api.Domain;

public enum OrderStatus
{
    Placed,
    Paid,
    Preparing,
    Shipped,
    OutForDelivery,
    Delivered,
    Cancelled
}

/// <summary>A placed order. Item price/name/image are snapshots taken at checkout (see <see cref="OrderItem"/>).</summary>
public sealed class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public required decimal TotalAmount { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<OrderItem> Items { get; init; } = [];
}
