using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Events;

/// <summary>Queue URL for a specific event type's SQS subscription; registered per TEvent so multiple consumers can coexist.</summary>
public sealed record SqsQueueUrl<TEvent>(string Url) where TEvent : IntegrationEvent;

/// <summary>
/// Long-polls an SQS queue for messages of type <typeparamref name="TEvent"/>, dispatches each to a scoped
/// <see cref="IEventHandler{TEvent}"/>, and deletes the message once handled. Failed messages are left in the
/// queue to retry/eventually land in a DLQ, matching AWS's standard at-least-once delivery semantics.
/// </summary>
public sealed class SqsEventConsumer<TEvent>(
    IAmazonSQS sqsClient,
    SqsQueueUrl<TEvent> queueUrl,
    IServiceScopeFactory scopeFactory,
    ILogger<SqsEventConsumer<TEvent>> logger) : BackgroundService
    where TEvent : IntegrationEvent
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting SQS consumer for {EventType} on {QueueUrl}", typeof(TEvent).Name, queueUrl.Url);

        while (!stoppingToken.IsCancellationRequested)
        {
            ReceiveMessageResponse response;
            try
            {
                response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl.Url,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10
                }, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to poll SQS queue {QueueUrl}", queueUrl.Url);
                continue;
            }

            foreach (var message in response.Messages)
            {
                await ProcessMessageAsync(message, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(Message message, CancellationToken ct)
    {
        try
        {
            var @event = JsonSerializer.Deserialize<TEvent>(message.Body, JsonOptions);
            if (@event is not null)
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
                await handler.HandleAsync(@event, ct);
            }

            await sqsClient.DeleteMessageAsync(queueUrl.Url, message.ReceiptHandle, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process SQS message {MessageId} for {EventType}", message.MessageId, typeof(TEvent).Name);
        }
    }
}
