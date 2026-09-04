using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using CartService.Api.Contracts;
using CartService.Api.Services;

namespace CartService.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart").WithTags("Cart").RequireAuthorization(Policies.Customer);

        group.MapGet("/", async (HttpContext http, CartManager manager, CancellationToken ct) =>
            Results.Ok(await manager.GetCartAsync(RequireUserId(http), ct)));

        group.MapPost("/items", async (AddCartItemRequest request, HttpContext http, CartManager manager, CancellationToken ct) =>
            Results.Ok(await manager.AddItemAsync(RequireUserId(http), request, ct)));

        group.MapPut("/items/{productId:guid}", async (Guid productId, UpdateCartItemRequest request, HttpContext http, CartManager manager, CancellationToken ct) =>
            Results.Ok(await manager.UpdateItemQuantityAsync(RequireUserId(http), productId, request, ct)));

        group.MapDelete("/items/{productId:guid}", async (Guid productId, HttpContext http, CartManager manager, CancellationToken ct) =>
            Results.Ok(await manager.RemoveItemAsync(RequireUserId(http), productId, ct)));

        group.MapDelete("/", async (HttpContext http, CartManager manager, CancellationToken ct) =>
        {
            await manager.ClearCartAsync(RequireUserId(http), ct);
            return Results.NoContent();
        });

        // Internal service-to-service endpoints called by Order Service during checkout orchestration.
        var internalGroup = app.MapGroup("/api/cart").WithTags("Cart");

        internalGroup.MapGet("/users/{userId:guid}", async (Guid userId, CartManager manager, CancellationToken ct) =>
            Results.Ok(await manager.GetCartAsync(userId, ct)));

        internalGroup.MapDelete("/users/{userId:guid}", async (Guid userId, CartManager manager, CancellationToken ct) =>
        {
            await manager.ClearCartAsync(userId, ct);
            return Results.NoContent();
        });
    }

    private static Guid RequireUserId(HttpContext http) =>
        http.User.GetUserId() ?? throw new UnauthorizedApiException("Missing or invalid user identity.");
}
