namespace RecommendationService.Api.Clients;

public sealed record CatalogProductCandidate(
    Guid Id,
    string Name,
    string PrimaryImageUrl,
    decimal EffectivePrice,
    double AverageRating,
    bool InStock);

/// <summary>Read-only Catalog lookups needed to turn raw product ids into displayable recommendation cards.</summary>
public interface ICatalogServiceClient
{
    /// <summary>Returns the product's category slug, or null if the product doesn't exist.</summary>
    Task<string?> GetCategorySlugAsync(Guid productId, CancellationToken ct);

    Task<IReadOnlyList<CatalogProductCandidate>> GetProductsByCategoryAsync(string categorySlug, int take, CancellationToken ct);

    /// <summary>Returns null if the product no longer exists (e.g. deactivated since it was viewed/purchased).</summary>
    Task<CatalogProductCandidate?> GetProductAsync(Guid productId, CancellationToken ct);
}
