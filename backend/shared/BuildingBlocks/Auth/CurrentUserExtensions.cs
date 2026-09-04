using System.Security.Claims;

namespace BuildingBlocks.Auth;

/// <summary>Reads the authenticated user's id/role from JWT claims, wherever a request is handled.</summary>
public static class CurrentUserExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin);
}
