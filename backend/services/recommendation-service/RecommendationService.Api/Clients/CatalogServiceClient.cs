using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Common;

namespace RecommendationService.Api.Clients;

/// <summary>
/// Calls Catalog Service's internal/public lookup endpoints. Base address is resolved via Aspire
/// service discovery ("http://catalog-service" matches the AppHost resource name); in
/// non-Aspire environments (e.g. AWS ECS), configure "Services:CatalogService:BaseUrl".
/// </summary>
public sealed class CatalogServiceClient(HttpClient httpClient) : ICatalogServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string?> GetCategorySlugAsync(Guid productId, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync($"/api/catalog/products/id/{productId}/category", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>(JsonOptions, ct);
        return category?.Slug;
    }

    public async Task<IReadOnlyList<CatalogProductCandidate>> GetProductsByCategoryAsync(string categorySlug, int take, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync(
            $"/api/catalog/products?category={Uri.EscapeDataString(categorySlug)}&pageSize={take}", ct);
        response.EnsureSuccessStatusCode();

        var page = await response.Content.ReadFromJsonAsync<PagedResult<CatalogProductSummary>>(JsonOptions, ct);
        return page?.Items.Select(ToCandidate).ToList() ?? [];
    }

    public async Task<CatalogProductCandidate?> GetProductAsync(Guid productId, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync($"/api/catalog/products/id/{productId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var summary = await response.Content.ReadFromJsonAsync<CatalogProductSummary>(JsonOptions, ct);
        return summary is null ? null : ToCandidate(summary);
    }

    private static CatalogProductCandidate ToCandidate(CatalogProductSummary summary) => new(
        summary.Id, summary.Name, summary.PrimaryImageUrl, summary.EffectivePrice, summary.AverageRating, summary.InStock);

    // Mirrors CatalogService.Api.Contracts.CategoryResponse's JSON shape without a project reference.
    private sealed record CategoryResponse(Guid Id, string Name, string Slug, string? IconUrl);

    // Mirrors CatalogService.Api.Contracts.ProductSummaryResponse's JSON shape without a project reference.
    private sealed record CatalogProductSummary(
        Guid Id,
        string Name,
        string Slug,
        decimal Price,
        decimal EffectivePrice,
        int? DiscountPercent,
        string PrimaryImageUrl,
        double AverageRating,
        int RatingCount,
        int SoldCount,
        bool InStock);
}
