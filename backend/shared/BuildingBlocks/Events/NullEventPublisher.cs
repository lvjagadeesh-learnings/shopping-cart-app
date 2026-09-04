using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Events;

/// <summary>Local-dev/test fallback publisher — logs instead of calling AWS, so services run without SNS configured.</summary>
public sealed class NullEventPublisher(ILogger<NullEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
    {
        logger.LogInformation("[NullEventPublisher] Would publish {EventType} ({EventId})", @event.EventType, @event.EventId);
        return Task.CompletedTask;
    }
}
