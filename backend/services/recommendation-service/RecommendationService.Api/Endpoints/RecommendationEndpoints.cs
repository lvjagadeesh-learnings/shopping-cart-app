using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using RecommendationService.Api.Contracts;
using RecommendationService.Api.Services;

namespace RecommendationService.Api.Endpoints;

public static class RecommendationEndpoints
{
    public static void MapRecommendationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recommendations").WithTags("Recommendations");

        group.MapPost("/views", async (RecordViewRequest request, HttpContext context, RecommendationManager manager, CancellationToken ct) =>
        {
            var userId = context.User.GetUserId() ?? throw new UnauthorizedApiException("Missing user id claim.");
            await manager.RecordViewAsync(userId, request.ProductId, ct);
            return Results.NoContent();
        }).RequireAuthorization(Policies.Customer);

        // Public — related/trending widgets are shown to anonymous browsers too.
        group.MapGet("/related/{productId:guid}", async (Guid productId, RecommendationManager manager, CancellationToken ct) =>
            Results.Ok(await manager.GetRelatedAsync(productId, ct)));

        group.MapGet("/trending", async (RecommendationManager manager, CancellationToken ct, int take = 10) =>
            Results.Ok(await manager.GetTrendingAsync(take, ct)));
    }
}
