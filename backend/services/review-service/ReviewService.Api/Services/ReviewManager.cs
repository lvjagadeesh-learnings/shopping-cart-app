using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using ReviewService.Api.Clients;
using ReviewService.Api.Contracts;
using ReviewService.Api.Data;
using ReviewService.Api.Domain;
using ReviewService.Api.Mapping;

namespace ReviewService.Api.Services;

public sealed class ReviewManager(ReviewDbContext db, ICatalogServiceClient catalogClient)
{
    public async Task<ReviewResponse> CreateAsync(Guid userId, Guid productId, CreateReviewRequest request, CancellationToken ct)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new ValidationApiException("Rating must be between 1 and 5.");
        }

        if (!await catalogClient.ProductExistsAsync(productId, ct))
        {
            throw new NotFoundException($"Product '{productId}' was not found.");
        }

        if (await db.Reviews.AnyAsync(r => r.ProductId == productId && r.UserId == userId, ct))
        {
            throw new ConflictException("You have already reviewed this product.");
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);

        return review.ToResponse();
    }

    public async Task<ProductReviewSummaryResponse> GetForProductAsync(Guid productId, CancellationToken ct)
    {
        var reviews = await db.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(ct);

        var average = reviews.Count == 0 ? 0d : reviews.Average(r => r.Rating);

        return new ProductReviewSummaryResponse(
            productId,
            Math.Round(average, 2),
            reviews.Count,
            reviews.Select(r => r.ToResponse()).ToList());
    }

    public async Task DeleteAsync(Guid userId, Guid reviewId, CancellationToken ct)
    {
        var review = await db.Reviews.SingleOrDefaultAsync(r => r.Id == reviewId, ct)
            ?? throw new NotFoundException("Review not found.");

        if (review.UserId != userId)
        {
            throw new NotFoundException("Review not found.");
        }

        db.Reviews.Remove(review);
        await db.SaveChangesAsync(ct);
    }
}
