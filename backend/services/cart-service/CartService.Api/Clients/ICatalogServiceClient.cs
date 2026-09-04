using CartService.Api.Contracts;

namespace CartService.Api.Clients;

public interface ICatalogServiceClient
{
    Task<ProductLookupResult?> GetProductAsync(Guid productId, CancellationToken ct);
}
