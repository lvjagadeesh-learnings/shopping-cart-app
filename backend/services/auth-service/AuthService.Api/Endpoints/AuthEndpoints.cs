using AuthService.Api.Contracts;
using AuthService.Api.Services;
using BuildingBlocks.Auth;

namespace AuthService.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, AuthTokenService service, CancellationToken ct) =>
            Results.Ok(await service.RegisterAsync(request, ct)));

        group.MapPost("/login", async (LoginRequest request, AuthTokenService service, CancellationToken ct) =>
            Results.Ok(await service.LoginAsync(request, ct)));

        group.MapPost("/refresh", async (RefreshRequest request, AuthTokenService service, CancellationToken ct) =>
            Results.Ok(await service.RefreshAsync(request, ct)));

        group.MapPost("/logout", async (LogoutRequest request, AuthTokenService service, CancellationToken ct) =>
        {
            await service.LogoutAsync(request, ct);
            return Results.NoContent();
        });

        group.MapGet("/me", (HttpContext context) =>
        {
            var userId = context.User.GetUserId();
            return userId is null
                ? Results.Unauthorized()
                : Results.Ok(new
                {
                    id = userId,
                    email = context.User.FindFirstEmail(),
                    role = context.User.IsAdmin() ? Roles.Admin : Roles.Customer
                });
        }).RequireAuthorization();
    }

    private static string? FindFirstEmail(this System.Security.Claims.ClaimsPrincipal user) =>
        user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        ?? user.FindFirst("email")?.Value;
}
