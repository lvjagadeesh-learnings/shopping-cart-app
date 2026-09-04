namespace BuildingBlocks.Events;

/// <summary>Publishes integration events; backed by SNS in AWS, a no-op logger locally/in tests.</summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
}
