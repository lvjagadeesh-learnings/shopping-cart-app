using BuildingBlocks.Events;
using NotificationService.Api.Services;

namespace NotificationService.Api.EventHandlers;

/// <summary>Dev fallback: logs a simulated "order status update email" instead of calling AWS SES.</summary>
public sealed class OrderStatusChangedNotificationHandler(NotificationManager manager, ILogger<OrderStatusChangedNotificationHandler> logger)
    : IEventHandler<OrderStatusChangedEvent>
{
    public async Task HandleAsync(OrderStatusChangedEvent @event, CancellationToken cancellationToken)
    {
        var message = $"Your order {@event.OrderId} is now {@event.NewStatus}.";
        await manager.LogAsync(@event.UserId, @event.EventType, "Order status updated", message, cancellationToken);
        logger.LogInformation("[Simulated email] To user {UserId}: {Message}", @event.UserId, message);
    }
}
