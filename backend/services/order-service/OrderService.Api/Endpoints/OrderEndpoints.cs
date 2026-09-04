using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using OrderService.Api.Contracts;
using OrderService.Api.Services;

namespace OrderService.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization(Policies.Customer);

        group.MapPost("/checkout", async (HttpContext http, OrderManager manager, CancellationToken ct) =>
            Results.Ok(await manager.CheckoutAsync(RequireUserId(http), ct)));

        group.MapGet("/", async (HttpContext http, OrderManager manager, CancellationToken ct) =>
            Results.Ok(await manager.GetOrderHistoryAsync(RequireUserId(http), ct)));

        group.MapGet("/{orderId:guid}", async (Guid orderId, HttpContext http, OrderManager manager, CancellationToken ct) =>
            Results.Ok(await manager.GetByIdAsync(RequireUserId(http), orderId, ct)));

        // Admin-only fulfillment workflow transitions (Placed -> Paid -> Preparing -> Shipped -> OutForDelivery -> Delivered/Cancelled).
        group.MapPut("/{orderId:guid}/status", async (Guid orderId, UpdateOrderStatusRequest request, OrderManager manager, CancellationToken ct) =>
                Results.Ok(await manager.UpdateStatusAsync(orderId, request, ct)))
            .RequireAuthorization(Policies.Admin);
    }

    private static Guid RequireUserId(HttpContext http) =>
        http.User.GetUserId() ?? throw new UnauthorizedApiException("Missing or invalid user identity.");
}
