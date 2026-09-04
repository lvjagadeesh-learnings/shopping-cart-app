using OrderService.Api.Contracts;

namespace OrderService.Api.Clients;

public interface ICartServiceClient
{
    Task<CartLookupResponse?> GetCartAsync(Guid userId, CancellationToken ct);
    Task ClearCartAsync(Guid userId, CancellationToken ct);
}
