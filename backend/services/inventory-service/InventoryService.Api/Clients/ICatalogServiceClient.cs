namespace InventoryService.Api.Clients;

public interface ICatalogServiceClient
{
    /// <summary>Lists every active product id currently known to Catalog Service, used to seed the stock ledger.</summary>
    Task<IReadOnlyList<Guid>> ListProductIdsAsync(CancellationToken ct);
}
