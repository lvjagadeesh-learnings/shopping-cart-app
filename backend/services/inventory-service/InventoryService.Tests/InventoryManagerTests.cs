using BuildingBlocks.Common;
using InventoryService.Api.Contracts;
using InventoryService.Api.Data;
using InventoryService.Api.Domain;
using InventoryService.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Tests;

public class InventoryManagerTests
{
    private static readonly Guid OrderId = Guid.NewGuid();

    private static InventoryManager CreateSut(out InventoryDbContext db)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        db = new InventoryDbContext(options);
        return new InventoryManager(db);
    }

    private static async Task<Guid> SeedStockAsync(InventoryDbContext db, int onHand = 100)
    {
        var productId = Guid.NewGuid();
        db.StockLevels.Add(new StockLevel { ProductId = productId, QuantityOnHand = onHand });
        await db.SaveChangesAsync(CancellationToken.None);
        return productId;
    }

    [Fact]
    public async Task ReserveAsync_SufficientStock_CreatesReservationAndReducesAvailable()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db, onHand: 10);

        var result = await sut.ReserveAsync(
            new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 3)]), CancellationToken.None);

        var reservation = Assert.Single(result);
        Assert.Equal(ReservationStatus.Reserved.ToString(), reservation.Status);
        Assert.Equal(3, reservation.Quantity);

        var stock = await sut.GetStockLevelAsync(productId, CancellationToken.None);
        Assert.Equal(10, stock.QuantityOnHand);
        Assert.Equal(3, stock.QuantityReserved);
        Assert.Equal(7, stock.QuantityAvailable);
    }

    [Fact]
    public async Task ReserveAsync_InsufficientStock_ThrowsConflictExceptionAndReservesNothing()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db, onHand: 2);

        await Assert.ThrowsAsync<ConflictException>(() =>
            sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 5)]), CancellationToken.None));

        var stock = await sut.GetStockLevelAsync(productId, CancellationToken.None);
        Assert.Equal(0, stock.QuantityReserved);
    }

    [Fact]
    public async Task ReserveAsync_OneOfManyItemsInsufficient_ReservesNoneOfThem()
    {
        var sut = CreateSut(out var db);
        var plentiful = await SeedStockAsync(db, onHand: 100);
        var scarce = await SeedStockAsync(db, onHand: 1);

        await Assert.ThrowsAsync<ConflictException>(() =>
            sut.ReserveAsync(
                new ReserveStockRequest(OrderId, [new ReserveStockItem(plentiful, 5), new ReserveStockItem(scarce, 5)]),
                CancellationToken.None));

        var plentifulStock = await sut.GetStockLevelAsync(plentiful, CancellationToken.None);
        Assert.Equal(0, plentifulStock.QuantityReserved);
    }

    [Fact]
    public async Task ReserveAsync_UnknownProduct_ThrowsConflictException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<ConflictException>(() =>
            sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(Guid.NewGuid(), 1)]), CancellationToken.None));
    }

    [Fact]
    public async Task ReserveAsync_ZeroOrNegativeQuantity_ThrowsValidationApiException()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db);

        await Assert.ThrowsAsync<ValidationApiException>(() =>
            sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 0)]), CancellationToken.None));
    }

    [Fact]
    public async Task ReserveAsync_NoItems_ThrowsValidationApiException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<ValidationApiException>(() =>
            sut.ReserveAsync(new ReserveStockRequest(OrderId, []), CancellationToken.None));
    }

    [Fact]
    public async Task ReserveAsync_CalledTwiceForSameOrder_IsIdempotentAndDoesNotDoubleReserve()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db, onHand: 10);

        await sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 3)]), CancellationToken.None);
        await sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 3)]), CancellationToken.None);

        var stock = await sut.GetStockLevelAsync(productId, CancellationToken.None);
        Assert.Equal(3, stock.QuantityReserved);
    }

    [Fact]
    public async Task CommitAsync_ReservedOrder_DeductsOnHandAndMarksCommitted()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db, onHand: 10);
        await sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 4)]), CancellationToken.None);

        var result = await sut.CommitAsync(OrderId, CancellationToken.None);

        var reservation = Assert.Single(result);
        Assert.Equal(ReservationStatus.Committed.ToString(), reservation.Status);

        var stock = await sut.GetStockLevelAsync(productId, CancellationToken.None);
        Assert.Equal(6, stock.QuantityOnHand);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(6, stock.QuantityAvailable);
    }

    [Fact]
    public async Task CommitAsync_UnknownOrder_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.CommitAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task CommitAsync_CalledTwice_IsIdempotentAndDoesNotDoubleDeduct()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db, onHand: 10);
        await sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 4)]), CancellationToken.None);

        await sut.CommitAsync(OrderId, CancellationToken.None);
        await sut.CommitAsync(OrderId, CancellationToken.None);

        var stock = await sut.GetStockLevelAsync(productId, CancellationToken.None);
        Assert.Equal(6, stock.QuantityOnHand);
    }

    [Fact]
    public async Task ReleaseAsync_ReservedOrder_RestoresAvailableWithoutChangingOnHand()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db, onHand: 10);
        await sut.ReserveAsync(new ReserveStockRequest(OrderId, [new ReserveStockItem(productId, 4)]), CancellationToken.None);

        var result = await sut.ReleaseAsync(OrderId, CancellationToken.None);

        var reservation = Assert.Single(result);
        Assert.Equal(ReservationStatus.Released.ToString(), reservation.Status);

        var stock = await sut.GetStockLevelAsync(productId, CancellationToken.None);
        Assert.Equal(10, stock.QuantityOnHand);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(10, stock.QuantityAvailable);
    }

    [Fact]
    public async Task ReleaseAsync_UnknownOrder_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.ReleaseAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetStockLevelAsync_UnknownProduct_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetStockLevelAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task AdjustStockAsync_NewProduct_CreatesStockLevel()
    {
        var sut = CreateSut(out _);
        var productId = Guid.NewGuid();

        var result = await sut.AdjustStockAsync(productId, new AdjustStockRequest(50), CancellationToken.None);

        Assert.Equal(50, result.QuantityOnHand);
        Assert.Equal(0, result.QuantityReserved);
    }

    [Fact]
    public async Task AdjustStockAsync_ExistingProduct_UpdatesQuantityOnHand()
    {
        var sut = CreateSut(out var db);
        var productId = await SeedStockAsync(db, onHand: 20);

        var result = await sut.AdjustStockAsync(productId, new AdjustStockRequest(75), CancellationToken.None);

        Assert.Equal(75, result.QuantityOnHand);
    }

    [Fact]
    public async Task AdjustStockAsync_NegativeQuantity_ThrowsValidationApiException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<ValidationApiException>(() =>
            sut.AdjustStockAsync(Guid.NewGuid(), new AdjustStockRequest(-1), CancellationToken.None));
    }
}
