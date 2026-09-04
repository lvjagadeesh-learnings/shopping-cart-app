using System.Text.Json;
using OrderService.Api.Contracts;

namespace OrderService.Api.Clients;

/// <summary>
/// Calls Cart Service's internal, unauthenticated user-scoped endpoints. Base address is resolved
/// via Aspire service discovery ("http://cart-service" matches the AppHost resource name); in
/// non-Aspire environments (e.g. AWS ECS/ALB), configure "Services:CartService:BaseUrl".
/// </summary>
public sealed class CartServiceClient(HttpClient httpClient) : ICartServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CartLookupResponse?> GetCartAsync(Guid userId, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync($"/api/cart/users/{userId}", ct);
        if (!response.IsSuccessStatusCode)
        {
            throw await DownstreamErrorMapper.ToApiExceptionAsync(response, ct);
        }

        return await response.Content.ReadFromJsonAsync<CartLookupResponse>(JsonOptions, ct);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken ct)
    {
        using var response = await httpClient.DeleteAsync($"/api/cart/users/{userId}", ct);
        if (!response.IsSuccessStatusCode)
        {
            throw await DownstreamErrorMapper.ToApiExceptionAsync(response, ct);
        }
    }
}
