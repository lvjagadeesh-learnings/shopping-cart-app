using BuildingBlocks.Common;
using CartService.Api.Clients;
using CartService.Api.Contracts;
using CartService.Api.Data;
using CartService.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CartService.Tests;

/// <summary>Fake Catalog client so Cart tests don't depend on a running Catalog Service.</summary>
public sealed class FakeCatalogServiceClient : ICatalogServiceClient
{
    private readonly Dictionary<Guid, ProductLookupResult> _products = [];

    public FakeCatalogServiceClient WithProduct(ProductLookupResult product)
    {
        _products[product.Id] = product;
        return this;
    }

    public Task<ProductLookupResult?> GetProductAsync(Guid productId, CancellationToken ct) =>
        Task.FromResult(_products.GetValueOrDefault(productId));
}

public class CartManagerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static CartManager CreateSut(out CartDbContext db, out FakeCatalogServiceClient catalogClient)
    {
        var options = new DbContextOptionsBuilder<CartDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        db = new CartDbContext(options);
        catalogClient = new FakeCatalogServiceClient();
        return new CartManager(db, catalogClient);
    }

    private static ProductLookupResult SampleProduct(Guid? id = null, decimal price = 19.99m, bool inStock = true) =>
        new(id ?? Guid.NewGuid(), "Sample Widget", "https://picsum.photos/seed/widget/480", price, inStock);

    [Fact]
    public async Task GetCartAsync_NoExistingCart_CreatesEmptyCart()
    {
        var sut = CreateSut(out _, out _);

        var cart = await sut.GetCartAsync(UserId, CancellationToken.None);

        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.Subtotal);
        Assert.Equal(0, cart.TotalItems);
    }

    [Fact]
    public async Task AddItemAsync_NewProduct_AddsLineItem()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct(price: 25m);
        catalog.WithProduct(product);

        var cart = await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 2), CancellationToken.None);

        var item = Assert.Single(cart.Items);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(25m, item.UnitPrice);
        Assert.Equal(50m, item.LineTotal);
        Assert.Equal(50m, cart.Subtotal);
        Assert.Equal(2, cart.TotalItems);
    }

    [Fact]
    public async Task AddItemAsync_ExistingProduct_IncrementsQuantityInsteadOfDuplicating()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct(price: 10m);
        catalog.WithProduct(product);

        await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 1), CancellationToken.None);
        var cart = await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 3), CancellationToken.None);

        var item = Assert.Single(cart.Items);
        Assert.Equal(4, item.Quantity);
    }

    [Fact]
    public async Task AddItemAsync_UnknownProduct_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _, out _);

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.AddItemAsync(UserId, new AddCartItemRequest(Guid.NewGuid(), 1), CancellationToken.None));
    }

    [Fact]
    public async Task AddItemAsync_OutOfStockProduct_ThrowsConflictException()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct(inStock: false);
        catalog.WithProduct(product);

        await Assert.ThrowsAsync<ConflictException>(
            () => sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 1), CancellationToken.None));
    }

    [Fact]
    public async Task AddItemAsync_ZeroOrNegativeQuantity_ThrowsValidationApiException()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct();
        catalog.WithProduct(product);

        await Assert.ThrowsAsync<ValidationApiException>(
            () => sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 0), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_PositiveQuantity_UpdatesLine()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct(price: 5m);
        catalog.WithProduct(product);
        await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 1), CancellationToken.None);

        var cart = await sut.UpdateItemQuantityAsync(UserId, product.Id, new UpdateCartItemRequest(5), CancellationToken.None);

        var item = Assert.Single(cart.Items);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_ZeroQuantity_RemovesLine()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct();
        catalog.WithProduct(product);
        await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 1), CancellationToken.None);

        var cart = await sut.UpdateItemQuantityAsync(UserId, product.Id, new UpdateCartItemRequest(0), CancellationToken.None);

        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_NegativeQuantity_ThrowsValidationApiException()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct();
        catalog.WithProduct(product);
        await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 1), CancellationToken.None);

        await Assert.ThrowsAsync<ValidationApiException>(
            () => sut.UpdateItemQuantityAsync(UserId, product.Id, new UpdateCartItemRequest(-1), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_UnknownProduct_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _, out _);

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.UpdateItemQuantityAsync(UserId, Guid.NewGuid(), new UpdateCartItemRequest(1), CancellationToken.None));
    }

    [Fact]
    public async Task RemoveItemAsync_ExistingItem_RemovesIt()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct();
        catalog.WithProduct(product);
        await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 1), CancellationToken.None);

        var cart = await sut.RemoveItemAsync(UserId, product.Id, CancellationToken.None);

        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task ClearCartAsync_RemovesAllItems()
    {
        var sut = CreateSut(out var db, out var catalog);
        var p1 = SampleProduct();
        var p2 = SampleProduct();
        catalog.WithProduct(p1).WithProduct(p2);
        await sut.AddItemAsync(UserId, new AddCartItemRequest(p1.Id, 1), CancellationToken.None);
        await sut.AddItemAsync(UserId, new AddCartItemRequest(p2.Id, 1), CancellationToken.None);

        await sut.ClearCartAsync(UserId, CancellationToken.None);

        var cart = await sut.GetCartAsync(UserId, CancellationToken.None);
        Assert.Empty(cart.Items);
        Assert.Equal(0, await db.CartItems.CountAsync());
    }

    [Fact]
    public async Task GetCartAsync_IsScopedPerUser()
    {
        var sut = CreateSut(out _, out var catalog);
        var product = SampleProduct();
        catalog.WithProduct(product);
        await sut.AddItemAsync(UserId, new AddCartItemRequest(product.Id, 1), CancellationToken.None);

        var otherUserCart = await sut.GetCartAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(otherUserCart.Items);
    }
}
