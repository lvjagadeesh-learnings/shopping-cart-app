using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Common;
using BuildingBlocks.Events;
using RecommendationService.Api.Clients;
using RecommendationService.Api.Data;
using RecommendationService.Api.Domain;
using RecommendationService.Api.EventHandlers;
using RecommendationService.Api.Services;

namespace RecommendationService.Tests;

public class RecommendationManagerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private sealed class FakeCatalogServiceClient : ICatalogServiceClient
    {
        public Dictionary<Guid, string> CategorySlugsByProductId { get; } = [];
        public Dictionary<string, List<CatalogProductCandidate>> CandidatesByCategorySlug { get; } = [];
        public Dictionary<Guid, CatalogProductCandidate> ProductsById { get; } = [];

        public Task<string?> GetCategorySlugAsync(Guid productId, CancellationToken ct) =>
            Task.FromResult(CategorySlugsByProductId.GetValueOrDefault(productId));

        public Task<IReadOnlyList<CatalogProductCandidate>> GetProductsByCategoryAsync(string categorySlug, int take, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<CatalogProductCandidate>>(
                CandidatesByCategorySlug.GetValueOrDefault(categorySlug, []).Take(take).ToList());

        public Task<CatalogProductCandidate?> GetProductAsync(Guid productId, CancellationToken ct) =>
            Task.FromResult(ProductsById.GetValueOrDefault(productId));
    }

    private static CatalogProductCandidate Candidate(Guid id) =>
        new(id, $"Product {id:N}", "https://picsum.photos/seed/x/200", 9.99m, 4.5, true);

    private static (RecommendationDbContext Db, RecommendationManager Manager, FakeCatalogServiceClient Catalog) CreateSut()
    {
        var options = new DbContextOptionsBuilder<RecommendationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new RecommendationDbContext(options);
        var catalog = new FakeCatalogServiceClient();
        var manager = new RecommendationManager(db, catalog);
        return (db, manager, catalog);
    }

    [Fact]
    public async Task RecordViewAsync_PersistsView()
    {
        var (db, manager, _) = CreateSut();

        await manager.RecordViewAsync(UserId, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(1, await db.ProductViews.CountAsync());
    }

    [Fact]
    public async Task GetRelatedAsync_UnknownProduct_ThrowsNotFoundException()
    {
        var (_, manager, _) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => manager.GetRelatedAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetRelatedAsync_ExcludesSelfAndReturnsCategoryCandidates()
    {
        var (_, manager, catalog) = CreateSut();
        var productId = Guid.NewGuid();
        catalog.CategorySlugsByProductId[productId] = "electronics";
        catalog.CandidatesByCategorySlug["electronics"] =
            [Candidate(productId), .. Enumerable.Range(0, 8).Select(_ => Candidate(Guid.NewGuid()))];

        var related = await manager.GetRelatedAsync(productId, CancellationToken.None);

        Assert.DoesNotContain(related, r => r.ProductId == productId);
        Assert.Equal(8, related.Count);
    }

    [Fact]
    public async Task GetRelatedAsync_FewCategoryCandidates_FallsBackToTrending()
    {
        var (db, manager, catalog) = CreateSut();
        var productId = Guid.NewGuid();
        var trendingId = Guid.NewGuid();

        catalog.CategorySlugsByProductId[productId] = "books";
        catalog.CandidatesByCategorySlug["books"] = [];
        catalog.ProductsById[trendingId] = Candidate(trendingId);

        db.ProductViews.Add(new ProductView { UserId = UserId, ProductId = trendingId });
        await db.SaveChangesAsync(CancellationToken.None);

        var related = await manager.GetRelatedAsync(productId, CancellationToken.None);

        Assert.Contains(related, r => r.ProductId == trendingId);
    }

    [Fact]
    public async Task GetTrendingAsync_WeightsPurchasesHigherThanViews()
    {
        var (db, manager, catalog) = CreateSut();
        var popularByPurchase = Guid.NewGuid();
        var popularByView = Guid.NewGuid();
        catalog.ProductsById[popularByPurchase] = Candidate(popularByPurchase);
        catalog.ProductsById[popularByView] = Candidate(popularByView);

        db.ProductPurchases.Add(new ProductPurchase { UserId = UserId, ProductId = popularByPurchase, Quantity = 1 });
        for (var i = 0; i < 2; i++)
        {
            db.ProductViews.Add(new ProductView { UserId = UserId, ProductId = popularByView });
        }
        await db.SaveChangesAsync(CancellationToken.None);

        var trending = await manager.GetTrendingAsync(10, CancellationToken.None);

        Assert.Equal(popularByPurchase, trending[0].ProductId);
    }

    [Fact]
    public async Task GetTrendingAsync_SkipsProductsNoLongerInCatalog()
    {
        var (db, manager, catalog) = CreateSut();
        var deletedProductId = Guid.NewGuid();
        // Intentionally not registered in catalog.ProductsById, simulating a deactivated product.
        db.ProductViews.Add(new ProductView { UserId = UserId, ProductId = deletedProductId });
        await db.SaveChangesAsync(CancellationToken.None);

        var trending = await manager.GetTrendingAsync(10, CancellationToken.None);

        Assert.Empty(trending);
    }

    [Fact]
    public async Task OrderPlacedRecommendationHandler_PersistsOnePurchaseRowPerLineItem()
    {
        var options = new DbContextOptionsBuilder<RecommendationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new RecommendationDbContext(options);
        var handler = new OrderPlacedRecommendationHandler(db);

        await handler.HandleAsync(new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = UserId,
            TotalAmount = 40m,
            Items =
            [
                new OrderPlacedLineItem(Guid.NewGuid(), 2, 10m),
                new OrderPlacedLineItem(Guid.NewGuid(), 1, 20m)
            ]
        }, CancellationToken.None);

        Assert.Equal(2, await db.ProductPurchases.CountAsync());
    }
}
