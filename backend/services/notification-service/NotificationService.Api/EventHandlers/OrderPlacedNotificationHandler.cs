using BuildingBlocks.Events;
using NotificationService.Api.Services;

namespace NotificationService.Api.EventHandlers;

/// <summary>Dev fallback: logs a simulated "order confirmation email" instead of calling AWS SES.</summary>
public sealed class OrderPlacedNotificationHandler(NotificationManager manager, ILogger<OrderPlacedNotificationHandler> logger)
    : IEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken cancellationToken)
    {
        var message = $"Your order {@event.OrderId} for {@event.TotalAmount:C} has been placed.";
        await manager.LogAsync(@event.UserId, @event.EventType, "Order placed", message, cancellationToken);
        logger.LogInformation("[Simulated email] To user {UserId}: {Message}", @event.UserId, message);
    }
}
