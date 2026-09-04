using OrderService.Api.Contracts;

namespace OrderService.Api.Clients;

public interface IInventoryServiceClient
{
    Task ReserveAsync(Guid orderId, IReadOnlyList<ReserveStockItem> items, CancellationToken ct);
    Task CommitAsync(Guid orderId, CancellationToken ct);
    Task ReleaseAsync(Guid orderId, CancellationToken ct);
}
