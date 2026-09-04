namespace BuildingBlocks.Events;

/// <summary>Published by Order Service once checkout succeeds; fans out to Inventory, Notification, Recommendation.</summary>
public sealed record OrderPlacedEvent : IntegrationEvent
{
    public override string EventType => "OrderPlaced";

    public required Guid OrderId { get; init; }
    public required Guid UserId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required IReadOnlyList<OrderPlacedLineItem> Items { get; init; }
}

public sealed record OrderPlacedLineItem(Guid ProductId, int Quantity, decimal UnitPrice);

/// <summary>Published by Order Service on every status transition (Placed -> ... -> Delivered/Cancelled).</summary>
public sealed record OrderStatusChangedEvent : IntegrationEvent
{
    public override string EventType => "OrderStatusChanged";

    public required Guid OrderId { get; init; }
    public required Guid UserId { get; init; }
    public required string NewStatus { get; init; }
}
