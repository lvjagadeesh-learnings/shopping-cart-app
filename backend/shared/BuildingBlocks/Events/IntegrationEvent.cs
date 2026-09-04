namespace BuildingBlocks.Events;

/// <summary>Base contract for all cross-service integration events published over SNS.</summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Used as the SNS message attribute subscribers filter on (e.g. "OrderPlaced").</summary>
    public abstract string EventType { get; }
}
