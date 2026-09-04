using BuildingBlocks.Common;
using BuildingBlocks.Events;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Clients;
using OrderService.Api.Contracts;
using OrderService.Api.Data;
using OrderService.Api.Domain;
using OrderService.Api.Mapping;

namespace OrderService.Api.Services;

public sealed class OrderManager(
    OrderDbContext db,
    ICartServiceClient cartClient,
    IInventoryServiceClient inventoryClient,
    IPaymentServiceClient paymentClient,
    IEventPublisher eventPublisher)
{
    private static readonly OrderStatus[] ForwardSequence =
    [
        OrderStatus.Placed, OrderStatus.Paid, OrderStatus.Preparing,
        OrderStatus.Shipped, OrderStatus.OutForDelivery, OrderStatus.Delivered
    ];

    /// <summary>
    /// Checkout saga: reserve stock, authorize payment, persist the order, publish OrderPlaced,
    /// then clear the cart. Releases the stock reservation as a compensating action if payment
    /// is declined; nothing is persisted in that case.
    /// </summary>
    public async Task<OrderResponse> CheckoutAsync(Guid userId, CancellationToken ct)
    {
        var cart = await cartClient.GetCartAsync(userId, ct);
        if (cart is null || cart.Items.Count == 0)
        {
            throw new ValidationApiException("Cart is empty.");
        }

        var orderId = Guid.NewGuid();
        var items = cart.Items
            .Select(i => new OrderItem
            {
                OrderId = orderId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductImageUrl = i.ProductImageUrl,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            })
            .ToList();
        var totalAmount = items.Sum(i => i.UnitPrice * i.Quantity);

        await inventoryClient.ReserveAsync(orderId, items.Select(i => new ReserveStockItem(i.ProductId, i.Quantity)).ToList(), ct);

        var payment = await paymentClient.AuthorizeAsync(orderId, totalAmount, "USD", ct);
        if (payment.Status != "Authorized")
        {
            await inventoryClient.ReleaseAsync(orderId, ct);
            throw new ConflictException($"Payment was declined: {payment.FailureReason ?? "unknown reason"}.");
        }

        await inventoryClient.CommitAsync(orderId, ct);

        var order = new Order { Id = orderId, UserId = userId, TotalAmount = totalAmount, Status = OrderStatus.Placed };
        db.Orders.Add(order);
        db.OrderItems.AddRange(items);
        db.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = orderId, Status = OrderStatus.Placed });
        await db.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new OrderPlacedEvent
        {
            OrderId = orderId,
            UserId = userId,
            TotalAmount = totalAmount,
            Items = items.Select(i => new OrderPlacedLineItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList()
        }, ct);

        await cartClient.ClearCartAsync(userId, ct);

        return order.ToResponse(items);
    }

    public async Task<OrderResponse> GetByIdAsync(Guid userId, Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        return order.ToResponse(order.Items);
    }

    public async Task<IReadOnlyList<OrderSummaryResponse>> GetOrderHistoryAsync(Guid userId, CancellationToken ct)
    {
        var orders = await db.Orders.Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);

        return orders.Select(o => o.ToSummaryResponse()).ToList();
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            throw new ValidationApiException($"Unknown order status '{request.Status}'.");
        }

        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        if (!IsValidTransition(order.Status, newStatus))
        {
            throw new ConflictException($"Cannot transition order from '{order.Status}' to '{newStatus}'.");
        }

        order.Status = newStatus;
        order.UpdatedAtUtc = DateTimeOffset.UtcNow;
        db.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = orderId, Status = newStatus });
        await db.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new OrderStatusChangedEvent
        {
            OrderId = orderId,
            UserId = order.UserId,
            NewStatus = newStatus.ToString()
        }, ct);

        return order.ToResponse(order.Items);
    }

    /// <summary>Cancellation is allowed from any non-terminal state; otherwise only the next step in the fulfillment sequence is valid.</summary>
    private static bool IsValidTransition(OrderStatus current, OrderStatus next)
    {
        if (current is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return false;
        }

        if (next == OrderStatus.Cancelled)
        {
            return true;
        }

        var currentIndex = Array.IndexOf(ForwardSequence, current);
        var nextIndex = Array.IndexOf(ForwardSequence, next);
        return nextIndex == currentIndex + 1;
    }
}
