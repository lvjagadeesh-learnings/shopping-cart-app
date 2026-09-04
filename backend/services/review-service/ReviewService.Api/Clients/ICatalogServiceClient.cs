namespace ReviewService.Api.Clients;

/// <summary>Minimal Catalog lookup — Review Service only needs to confirm a product exists before accepting a review.</summary>
public interface ICatalogServiceClient
{
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken ct);
}
