using System.Text.Json;
using BuildingBlocks.Common;

namespace InventoryService.Api.Clients;

/// <summary>
/// Calls Catalog Service's public product-listing endpoint. Base address is resolved via Aspire
/// service discovery ("http://catalog-service" matches the AppHost resource name); in
/// non-Aspire environments (e.g. AWS ECS), configure "Services:CatalogService:BaseUrl".
/// </summary>
public sealed class CatalogServiceClient(HttpClient httpClient) : ICatalogServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<Guid>> ListProductIdsAsync(CancellationToken ct)
    {
        var result = await httpClient.GetFromJsonAsync<PagedResult<CatalogProductSummary>>(
            "/api/catalog/products?page=1&pageSize=100", JsonOptions, ct);

        return result?.Items.Select(p => p.Id).ToList() ?? [];
    }

    // Mirrors the subset of CatalogService.Api.Contracts.ProductSummaryResponse's JSON shape we need,
    // without a project reference (each service owns its own contracts).
    private sealed record CatalogProductSummary(Guid Id);
}
