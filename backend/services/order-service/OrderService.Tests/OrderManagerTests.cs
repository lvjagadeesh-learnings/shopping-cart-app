using BuildingBlocks.Common;
using BuildingBlocks.Events;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Clients;
using OrderService.Api.Contracts;
using OrderService.Api.Data;
using OrderService.Api.Domain;
using OrderService.Api.Services;

namespace OrderService.Tests;

public class OrderManagerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();

    private static OrderManager CreateSut(
        out OrderDbContext db,
        out FakeCartServiceClient cartClient,
        out FakeInventoryServiceClient inventoryClient,
        out FakePaymentServiceClient paymentClient,
        out FakeEventPublisher eventPublisher)
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        db = new OrderDbContext(options);
        cartClient = new FakeCartServiceClient();
        inventoryClient = new FakeInventoryServiceClient();
        paymentClient = new FakePaymentServiceClient();
        eventPublisher = new FakeEventPublisher();
        return new OrderManager(db, cartClient, inventoryClient, paymentClient, eventPublisher);
    }

    private static CartLookupResponse NonEmptyCart() => new(
        Guid.NewGuid(),
        [new CartLookupItem(Guid.NewGuid(), ProductId, "Widget", "https://img/widget.png", 9.99m, 2, 19.98m)],
        19.98m,
        2);

    [Fact]
    public async Task CheckoutAsync_WithValidCart_PlacesOrderAndPublishesEvent()
    {
        var sut = CreateSut(out var db, out var cartClient, out var inventoryClient, out var paymentClient, out var eventPublisher);
        cartClient.Cart = NonEmptyCart();
        paymentClient.ResultStatus = "Authorized";

        var response = await sut.CheckoutAsync(UserId, CancellationToken.None);

        Assert.Equal("Placed", response.Status);
        Assert.Single(response.Items);
        Assert.Equal(19.98m, response.TotalAmount);
        Assert.True(inventoryClient.Committed);
        Assert.False(inventoryClient.Released);
        Assert.True(cartClient.Cleared);
        Assert.Single(eventPublisher.Published.OfType<OrderPlacedEvent>());
        Assert.Equal(1, await db.Orders.CountAsync());
    }

    [Fact]
    public async Task CheckoutAsync_WithEmptyCart_ThrowsValidationApiException()
    {
        var sut = CreateSut(out _, out var cartClient, out _, out _, out _);
        cartClient.Cart = new CartLookupResponse(Guid.NewGuid(), [], 0m, 0);

        await Assert.ThrowsAsync<ValidationApiException>(() => sut.CheckoutAsync(UserId, CancellationToken.None));
    }

    [Fact]
    public async Task CheckoutAsync_WithNoCart_ThrowsValidationApiException()
    {
        var sut = CreateSut(out _, out var cartClient, out _, out _, out _);
        cartClient.Cart = null;

        await Assert.ThrowsAsync<ValidationApiException>(() => sut.CheckoutAsync(UserId, CancellationToken.None));
    }

    [Fact]
    public async Task CheckoutAsync_WithDeclinedPayment_ReleasesStockAndThrowsConflict()
    {
        var sut = CreateSut(out var db, out var cartClient, out var inventoryClient, out var paymentClient, out var eventPublisher);
        cartClient.Cart = NonEmptyCart();
        paymentClient.ResultStatus = "Failed";
        paymentClient.FailureReason = "Card declined";

        await Assert.ThrowsAsync<ConflictException>(() => sut.CheckoutAsync(UserId, CancellationToken.None));

        Assert.True(inventoryClient.Released);
        Assert.False(inventoryClient.Committed);
        Assert.Empty(eventPublisher.Published);
        Assert.Equal(0, await db.Orders.CountAsync());
    }

    [Fact]
    public async Task CheckoutAsync_WhenInventoryReservationFails_PropagatesConflictException()
    {
        var sut = CreateSut(out _, out var cartClient, out var inventoryClient, out _, out _);
        cartClient.Cart = NonEmptyCart();
        inventoryClient.ThrowOnReserve = new ConflictException("Insufficient stock.");

        await Assert.ThrowsAsync<ConflictException>(() => sut.CheckoutAsync(UserId, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownOrder_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(UserId, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_OwnedByAnotherUser_ThrowsNotFoundException()
    {
        var sut = CreateSut(out var db, out _, out _, out _, out _);
        var order = new Order { UserId = Guid.NewGuid(), TotalAmount = 10m };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(UserId, order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WithOwnedOrder_ReturnsResponse()
    {
        var sut = CreateSut(out var db, out _, out _, out _, out _);
        var order = new Order { UserId = UserId, TotalAmount = 10m };
        db.Orders.Add(order);
        db.OrderItems.Add(new OrderItem { OrderId = order.Id, ProductId = ProductId, ProductName = "Widget", ProductImageUrl = "https://img/widget.png", UnitPrice = 10m, Quantity = 1 });
        await db.SaveChangesAsync();

        var response = await sut.GetByIdAsync(UserId, order.Id, CancellationToken.None);

        Assert.Equal(order.Id, response.Id);
        Assert.Single(response.Items);
    }

    [Fact]
    public async Task GetOrderHistoryAsync_ReturnsOnlyOwnedOrdersMostRecentFirst()
    {
        var sut = CreateSut(out var db, out _, out _, out _, out _);
        var older = new Order { UserId = UserId, TotalAmount = 5m, CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1) };
        var newer = new Order { UserId = UserId, TotalAmount = 8m, CreatedAtUtc = DateTimeOffset.UtcNow };
        var other = new Order { UserId = Guid.NewGuid(), TotalAmount = 3m };
        db.Orders.AddRange(older, newer, other);
        await db.SaveChangesAsync();

        var history = await sut.GetOrderHistoryAsync(UserId, CancellationToken.None);

        Assert.Equal(2, history.Count);
        Assert.Equal(newer.Id, history[0].Id);
        Assert.Equal(older.Id, history[1].Id);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidForwardTransition_Succeeds()
    {
        var sut = CreateSut(out var db, out _, out _, out _, out var eventPublisher);
        var order = new Order { UserId = UserId, TotalAmount = 10m, Status = OrderStatus.Placed };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var response = await sut.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Paid"), CancellationToken.None);

        Assert.Equal("Paid", response.Status);
        Assert.Single(eventPublisher.Published.OfType<OrderStatusChangedEvent>());
    }

    [Fact]
    public async Task UpdateStatusAsync_SkippingAStep_ThrowsConflictException()
    {
        var sut = CreateSut(out var db, out _, out _, out _, out _);
        var order = new Order { UserId = UserId, TotalAmount = 10m, Status = OrderStatus.Placed };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() =>
            sut.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Shipped"), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_CancelFromNonTerminalState_Succeeds()
    {
        var sut = CreateSut(out var db, out _, out _, out _, out _);
        var order = new Order { UserId = UserId, TotalAmount = 10m, Status = OrderStatus.Preparing };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var response = await sut.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Cancelled"), CancellationToken.None);

        Assert.Equal("Cancelled", response.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithUnknownStatus_ThrowsValidationApiException()
    {
        var sut = CreateSut(out var db, out _, out _, out _, out _);
        var order = new Order { UserId = UserId, TotalAmount = 10m };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ValidationApiException>(() =>
            sut.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("NotAStatus"), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_WithUnknownOrder_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateStatusAsync(Guid.NewGuid(), new UpdateOrderStatusRequest("Paid"), CancellationToken.None));
    }

    private sealed class FakeCartServiceClient : ICartServiceClient
    {
        public CartLookupResponse? Cart { get; set; }
        public bool Cleared { get; private set; }

        public Task<CartLookupResponse?> GetCartAsync(Guid userId, CancellationToken ct) => Task.FromResult(Cart);

        public Task ClearCartAsync(Guid userId, CancellationToken ct)
        {
            Cleared = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeInventoryServiceClient : IInventoryServiceClient
    {
        public bool Committed { get; private set; }
        public bool Released { get; private set; }
        public Exception? ThrowOnReserve { get; set; }

        public Task ReserveAsync(Guid orderId, IReadOnlyList<ReserveStockItem> items, CancellationToken ct) =>
            ThrowOnReserve is not null ? Task.FromException(ThrowOnReserve) : Task.CompletedTask;

        public Task CommitAsync(Guid orderId, CancellationToken ct)
        {
            Committed = true;
            return Task.CompletedTask;
        }

        public Task ReleaseAsync(Guid orderId, CancellationToken ct)
        {
            Released = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePaymentServiceClient : IPaymentServiceClient
    {
        public string ResultStatus { get; set; } = "Authorized";
        public string? FailureReason { get; set; }

        public Task<PaymentResult> AuthorizeAsync(Guid orderId, decimal amount, string currency, CancellationToken ct) =>
            Task.FromResult(new PaymentResult(Guid.NewGuid(), orderId, amount, currency, ResultStatus, FailureReason));
    }

    private sealed class FakeEventPublisher : IEventPublisher
    {
        public List<IntegrationEvent> Published { get; } = [];

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
        {
            Published.Add(@event);
            return Task.CompletedTask;
        }
    }
}
