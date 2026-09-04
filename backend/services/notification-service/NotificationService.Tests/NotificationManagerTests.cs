using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationService.Api.Data;
using NotificationService.Api.EventHandlers;
using NotificationService.Api.Services;
using BuildingBlocks.Common;
using BuildingBlocks.Events;

namespace NotificationService.Tests;

public class NotificationManagerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static NotificationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task LogAsync_PersistsNotification()
    {
        var db = CreateDb();
        var manager = new NotificationManager(db);

        await manager.LogAsync(UserId, "OrderPlaced", "Order placed", "Your order was placed.", CancellationToken.None);

        Assert.Equal(1, await db.NotificationLogs.CountAsync());
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyOwnNotificationsNewestFirst()
    {
        var db = CreateDb();
        var manager = new NotificationManager(db);
        await manager.LogAsync(UserId, "OrderPlaced", "Older", "older", CancellationToken.None);
        await manager.LogAsync(UserId, "OrderStatusChanged", "Newer", "newer", CancellationToken.None);
        await manager.LogAsync(Guid.NewGuid(), "OrderPlaced", "Other user", "other", CancellationToken.None);

        var results = await manager.GetForUserAsync(UserId, CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Newer", results[0].Title);
    }

    [Fact]
    public async Task MarkAsReadAsync_UpdatesOwnedNotification()
    {
        var db = CreateDb();
        var manager = new NotificationManager(db);
        await manager.LogAsync(UserId, "OrderPlaced", "Title", "message", CancellationToken.None);
        var notification = (await manager.GetForUserAsync(UserId, CancellationToken.None))[0];

        await manager.MarkAsReadAsync(UserId, notification.Id, CancellationToken.None);

        var updated = (await manager.GetForUserAsync(UserId, CancellationToken.None))[0];
        Assert.True(updated.IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_NotOwned_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var manager = new NotificationManager(db);
        await manager.LogAsync(Guid.NewGuid(), "OrderPlaced", "Title", "message", CancellationToken.None);
        var notification = (await manager.GetForUserAsync(await FirstUserIdAsync(db), CancellationToken.None))[0];

        await Assert.ThrowsAsync<NotFoundException>(() => manager.MarkAsReadAsync(UserId, notification.Id, CancellationToken.None));
    }

    [Fact]
    public async Task MarkAsReadAsync_UnknownNotification_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var manager = new NotificationManager(db);

        await Assert.ThrowsAsync<NotFoundException>(() => manager.MarkAsReadAsync(UserId, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task OrderPlacedNotificationHandler_LogsNotification()
    {
        var db = CreateDb();
        var manager = new NotificationManager(db);
        var handler = new OrderPlacedNotificationHandler(manager, NullLogger<OrderPlacedNotificationHandler>.Instance);

        await handler.HandleAsync(new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = UserId,
            TotalAmount = 25.50m,
            Items = [new OrderPlacedLineItem(Guid.NewGuid(), 1, 25.50m)]
        }, CancellationToken.None);

        var results = await manager.GetForUserAsync(UserId, CancellationToken.None);
        Assert.Single(results);
        Assert.Equal("OrderPlaced", results[0].EventType);
    }

    [Fact]
    public async Task OrderStatusChangedNotificationHandler_LogsNotification()
    {
        var db = CreateDb();
        var manager = new NotificationManager(db);
        var handler = new OrderStatusChangedNotificationHandler(manager, NullLogger<OrderStatusChangedNotificationHandler>.Instance);

        await handler.HandleAsync(new OrderStatusChangedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = UserId,
            NewStatus = "Shipped"
        }, CancellationToken.None);

        var results = await manager.GetForUserAsync(UserId, CancellationToken.None);
        Assert.Single(results);
        Assert.Contains("Shipped", results[0].Message);
    }

    private static async Task<Guid> FirstUserIdAsync(NotificationDbContext db) =>
        (await db.NotificationLogs.FirstAsync()).UserId;
}
