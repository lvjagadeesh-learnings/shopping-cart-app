using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CartService.Api.Contracts;

namespace CartService.Api.Clients;

/// <summary>
/// Calls Catalog Service's internal-lookup-by-id endpoint. Base address is resolved via Aspire
/// service discovery ("http://catalog-service" matches the AppHost resource name); in
/// non-Aspire environments (e.g. AWS ECS), configure "Services:CatalogService:BaseUrl".
/// </summary>
public sealed class CatalogServiceClient(HttpClient httpClient) : ICatalogServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ProductLookupResult?> GetProductAsync(Guid productId, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync($"/api/catalog/products/id/{productId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var summary = await response.Content.ReadFromJsonAsync<CatalogProductSummary>(JsonOptions, ct);
        return summary is null
            ? null
            : new ProductLookupResult(summary.Id, summary.Name, summary.PrimaryImageUrl, summary.EffectivePrice, summary.InStock);
    }

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
