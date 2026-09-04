using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Events;

public sealed class SnsOptions
{
    public const string SectionName = "Sns";

    /// <summary>ARN of the shared "OrderEvents" (or per-domain) SNS topic.</summary>
    public required string TopicArn { get; init; }
}

/// <summary>Publishes integration events to an SNS topic, tagged with EventType as a message attribute for subscriber filtering.</summary>
public sealed class SnsEventPublisher(IAmazonSimpleNotificationService snsClient, IOptions<SnsOptions> options, ILogger<SnsEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
    {
        var request = new PublishRequest
        {
            TopicArn = options.Value.TopicArn,
            Message = JsonSerializer.Serialize(@event, @event.GetType()),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["EventType"] = new() { DataType = "String", StringValue = @event.EventType }
            }
        };

        await snsClient.PublishAsync(request, cancellationToken);
        logger.LogInformation("Published {EventType} ({EventId}) to {TopicArn}", @event.EventType, @event.EventId, options.Value.TopicArn);
    }
}
