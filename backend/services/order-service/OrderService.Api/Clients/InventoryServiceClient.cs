using OrderService.Api.Contracts;

namespace OrderService.Api.Clients;

/// <summary>
/// Calls Inventory Service's internal reserve/commit/release endpoints used during checkout
/// orchestration. Base address is resolved via Aspire service discovery ("http://inventory-service");
/// configure "Services:InventoryService:BaseUrl" to override in non-Aspire environments.
/// </summary>
public sealed class InventoryServiceClient(HttpClient httpClient) : IInventoryServiceClient
{
    public async Task ReserveAsync(Guid orderId, IReadOnlyList<ReserveStockItem> items, CancellationToken ct)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/inventory/reserve", new ReserveStockRequest(orderId, items), ct);
        if (!response.IsSuccessStatusCode)
        {
            throw await DownstreamErrorMapper.ToApiExceptionAsync(response, ct);
        }
    }

    public async Task CommitAsync(Guid orderId, CancellationToken ct)
    {
        using var response = await httpClient.PostAsync($"/api/inventory/orders/{orderId}/commit", content: null, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw await DownstreamErrorMapper.ToApiExceptionAsync(response, ct);
        }
    }

    public async Task ReleaseAsync(Guid orderId, CancellationToken ct)
    {
        using var response = await httpClient.PostAsync($"/api/inventory/orders/{orderId}/release", content: null, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw await DownstreamErrorMapper.ToApiExceptionAsync(response, ct);
        }
    }
}
