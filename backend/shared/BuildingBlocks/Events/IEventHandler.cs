namespace BuildingBlocks.Events;

/// <summary>Handles one integration event type consumed from SQS. Resolved per-message in its own DI scope.</summary>
public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
