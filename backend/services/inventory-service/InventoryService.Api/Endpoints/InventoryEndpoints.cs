using BuildingBlocks.Auth;
using InventoryService.Api.Contracts;
using InventoryService.Api.Services;

namespace InventoryService.Api.Endpoints;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory");

        // Internal service-to-service endpoints called by Order Service during checkout orchestration.
        group.MapPost("/reserve", async (ReserveStockRequest request, InventoryManager manager, CancellationToken ct) =>
            Results.Ok(await manager.ReserveAsync(request, ct)));

        group.MapPost("/orders/{orderId:guid}/commit", async (Guid orderId, InventoryManager manager, CancellationToken ct) =>
            Results.Ok(await manager.CommitAsync(orderId, ct)));

        group.MapPost("/orders/{orderId:guid}/release", async (Guid orderId, InventoryManager manager, CancellationToken ct) =>
            Results.Ok(await manager.ReleaseAsync(orderId, ct)));

        // Admin-only stock visibility/adjustment.
        group.MapGet("/stock/{productId:guid}", async (Guid productId, InventoryManager manager, CancellationToken ct) =>
                Results.Ok(await manager.GetStockLevelAsync(productId, ct)))
            .RequireAuthorization(Policies.Admin);

        group.MapPut("/stock/{productId:guid}", async (Guid productId, AdjustStockRequest request, InventoryManager manager, CancellationToken ct) =>
                Results.Ok(await manager.AdjustStockAsync(productId, request, ct)))
            .RequireAuthorization(Policies.Admin);
    }
}
