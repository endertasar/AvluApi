using System.Security.Claims;

namespace AvluApi.Auth;

public static class ClaimsPrincipalExtensions
{
    public static long GetSiteId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue("site_id");
        return claim != null ? long.Parse(claim) : 0;
    }

    public static long GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue("sub");
        return sub != null ? long.Parse(sub) : 0;
    }

    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirstValue("role") ?? string.Empty;

    public static string GetUserType(this ClaimsPrincipal user) =>
        user.FindFirstValue("user_type") ?? string.Empty;
}
