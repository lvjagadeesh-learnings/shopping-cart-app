using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using ReviewService.Api.Contracts;
using ReviewService.Api.Services;

namespace ReviewService.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews").WithTags("Reviews");

        group.MapPost("/products/{productId:guid}", async (Guid productId, CreateReviewRequest request, HttpContext context, ReviewManager manager, CancellationToken ct) =>
        {
            var userId = context.User.GetUserId() ?? throw new UnauthorizedApiException("Missing user id claim.");
            var response = await manager.CreateAsync(userId, productId, request, ct);
            return Results.Created($"/api/reviews/products/{productId}", response);
        }).RequireAuthorization(Policies.Customer);

        // Public — anyone browsing a product can see its reviews without authenticating.
        group.MapGet("/products/{productId:guid}", async (Guid productId, ReviewManager manager, CancellationToken ct) =>
            Results.Ok(await manager.GetForProductAsync(productId, ct)));

        group.MapDelete("/{reviewId:guid}", async (Guid reviewId, HttpContext context, ReviewManager manager, CancellationToken ct) =>
        {
            var userId = context.User.GetUserId() ?? throw new UnauthorizedApiException("Missing user id claim.");
            await manager.DeleteAsync(userId, reviewId, ct);
            return Results.NoContent();
        }).RequireAuthorization(Policies.Customer);
    }
}
