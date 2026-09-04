using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Api.Clients;
using RecommendationService.Api.Contracts;
using RecommendationService.Api.Data;
using RecommendationService.Api.Domain;

namespace RecommendationService.Api.Services;

public sealed class RecommendationManager(RecommendationDbContext db, ICatalogServiceClient catalogClient)
{
    private const int RelatedTake = 8;
    private const int TrendingTake = 10;
    private static readonly TimeSpan TrendingWindow = TimeSpan.FromDays(30);

    public async Task RecordViewAsync(Guid userId, Guid productId, CancellationToken ct)
    {
        db.ProductViews.Add(new ProductView { UserId = userId, ProductId = productId });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Related-in-category, falling back to trending if the category has too few other products.</summary>
    public async Task<IReadOnlyList<RecommendedProductResponse>> GetRelatedAsync(Guid productId, CancellationToken ct)
    {
        var categorySlug = await catalogClient.GetCategorySlugAsync(productId, ct)
            ?? throw new NotFoundException($"Product '{productId}' was not found.");

        var candidates = await catalogClient.GetProductsByCategoryAsync(categorySlug, RelatedTake + 1, ct);
        var related = candidates.Where(c => c.Id != productId).Take(RelatedTake).ToList();

        if (related.Count >= RelatedTake)
        {
            return related.Select(ToResponse).ToList();
        }

        var trending = await GetTrendingAsync(RelatedTake, ct);
        var seen = related.Select(r => r.Id).ToHashSet();
        seen.Add(productId);

        foreach (var item in trending)
        {
            if (related.Count >= RelatedTake)
            {
                break;
            }

            if (seen.Add(item.ProductId))
            {
                related.Add(new CatalogProductCandidate(item.ProductId, item.Name, item.PrimaryImageUrl, item.EffectivePrice, item.AverageRating, item.InStock));
            }
        }

        return related.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<RecommendedProductResponse>> GetTrendingAsync(int take, CancellationToken ct)
    {
        var since = DateTimeOffset.UtcNow - TrendingWindow;

        var viewCounts = await db.ProductViews
            .Where(v => v.ViewedAtUtc >= since)
            .GroupBy(v => v.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var purchaseCounts = await db.ProductPurchases
            .Where(p => p.PurchasedAtUtc >= since)
            .GroupBy(p => p.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Sum(p => p.Quantity) })
            .ToListAsync(ct);

        // Purchases signal stronger intent than views, so they're weighted 3x when ranking "trending".
        var scores = new Dictionary<Guid, double>();
        foreach (var v in viewCounts)
        {
            scores[v.ProductId] = scores.GetValueOrDefault(v.ProductId) + v.Count;
        }
        foreach (var p in purchaseCounts)
        {
            scores[p.ProductId] = scores.GetValueOrDefault(p.ProductId) + p.Count * 3;
        }

        var rankedProductIds = scores
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .Take(take)
            .ToList();

        var results = new List<RecommendedProductResponse>();
        foreach (var productId in rankedProductIds)
        {
            var candidate = await catalogClient.GetProductAsync(productId, ct);
            if (candidate is not null)
            {
                results.Add(ToResponse(candidate));
            }
        }

        return results;
    }

    private static RecommendedProductResponse ToResponse(CatalogProductCandidate candidate) => new(
        candidate.Id, candidate.Name, candidate.PrimaryImageUrl, candidate.EffectivePrice, candidate.AverageRating, candidate.InStock);
}
