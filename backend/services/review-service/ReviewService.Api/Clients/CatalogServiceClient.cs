using System.Net;

namespace ReviewService.Api.Clients;

/// <summary>
/// Calls Catalog Service's internal-lookup-by-id endpoint. Base address is resolved via Aspire
/// service discovery ("http://catalog-service" matches the AppHost resource name); in
/// non-Aspire environments (e.g. AWS ECS), configure "Services:CatalogService:BaseUrl".
/// </summary>
public sealed class CatalogServiceClient(HttpClient httpClient) : ICatalogServiceClient
{
    public async Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync($"/api/catalog/products/id/{productId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}
