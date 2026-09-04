using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using NotificationService.Api.Services;

namespace NotificationService.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization(Policies.Customer);

        group.MapGet("/", async (HttpContext context, NotificationManager manager, CancellationToken ct) =>
        {
            var userId = context.User.GetUserId() ?? throw new UnauthorizedApiException("Missing user id claim.");
            return Results.Ok(await manager.GetForUserAsync(userId, ct));
        });

        group.MapPut("/{notificationId:guid}/read", async (Guid notificationId, HttpContext context, NotificationManager manager, CancellationToken ct) =>
        {
            var userId = context.User.GetUserId() ?? throw new UnauthorizedApiException("Missing user id claim.");
            await manager.MarkAsReadAsync(userId, notificationId, ct);
            return Results.NoContent();
        });
    }
}
