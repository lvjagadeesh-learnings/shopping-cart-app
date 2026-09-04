using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Common;
using ReviewService.Api.Clients;
using ReviewService.Api.Contracts;
using ReviewService.Api.Data;
using ReviewService.Api.Services;

namespace ReviewService.Tests;

public class ReviewManagerTests
{
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private sealed class FakeCatalogServiceClient(bool exists = true) : ICatalogServiceClient
    {
        public Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct) => Task.FromResult(exists);
    }

    private static (ReviewDbContext Db, ReviewManager Manager) CreateSut(bool productExists = true)
    {
        var options = new DbContextOptionsBuilder<ReviewDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ReviewDbContext(options);
        var manager = new ReviewManager(db, new FakeCatalogServiceClient(productExists));
        return (db, manager);
    }

    [Fact]
    public async Task CreateAsync_Valid_PersistsReview()
    {
        var (db, manager) = CreateSut();

        var response = await manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(5, "Great!"), CancellationToken.None);

        Assert.Equal(5, response.Rating);
        Assert.Equal(1, await db.Reviews.CountAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateAsync_InvalidRating_ThrowsValidationApiException(int rating)
    {
        var (_, manager) = CreateSut();

        await Assert.ThrowsAsync<ValidationApiException>(() =>
            manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(rating, null), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_UnknownProduct_ThrowsNotFoundException()
    {
        var (_, manager) = CreateSut(productExists: false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(4, null), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Duplicate_ThrowsConflictException()
    {
        var (_, manager) = CreateSut();
        await manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(4, "First"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(3, "Second"), CancellationToken.None));
    }

    [Fact]
    public async Task GetForProductAsync_ComputesAverageAndOrdersNewestFirst()
    {
        var (_, manager) = CreateSut();
        await manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(3, "ok"), CancellationToken.None);
        await manager.CreateAsync(Guid.NewGuid(), ProductId, new CreateReviewRequest(5, "great"), CancellationToken.None);

        var summary = await manager.GetForProductAsync(ProductId, CancellationToken.None);

        Assert.Equal(2, summary.ReviewCount);
        Assert.Equal(4d, summary.AverageRating);
        Assert.Equal("great", summary.Reviews[0].Comment);
    }

    [Fact]
    public async Task GetForProductAsync_NoReviews_ReturnsZeroedSummary()
    {
        var (_, manager) = CreateSut();

        var summary = await manager.GetForProductAsync(ProductId, CancellationToken.None);

        Assert.Equal(0, summary.ReviewCount);
        Assert.Equal(0d, summary.AverageRating);
        Assert.Empty(summary.Reviews);
    }

    [Fact]
    public async Task DeleteAsync_Owner_RemovesReview()
    {
        var (db, manager) = CreateSut();
        var review = await manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(4, null), CancellationToken.None);

        await manager.DeleteAsync(UserId, review.Id, CancellationToken.None);

        Assert.Equal(0, await db.Reviews.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsNotFoundException()
    {
        var (_, manager) = CreateSut();
        var review = await manager.CreateAsync(UserId, ProductId, new CreateReviewRequest(4, null), CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            manager.DeleteAsync(Guid.NewGuid(), review.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_UnknownReview_ThrowsNotFoundException()
    {
        var (_, manager) = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            manager.DeleteAsync(UserId, Guid.NewGuid(), CancellationToken.None));
    }
}
