using System.Security.Claims;

namespace AvluApi.Auth;

public class SiteContext : ISiteContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SiteContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public long SiteId
    {
        get
        {
            var claim = User?.FindFirstValue("site_id");
            return claim != null ? long.Parse(claim) : 0;
        }
    }

    public long UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User?.FindFirstValue("sub");
            return sub != null ? long.Parse(sub) : 0;
        }
    }

    public string Role => User?.FindFirstValue("role") ?? string.Empty;

    public string UserType => User?.FindFirstValue("user_type") ?? string.Empty;

    public bool IsAdmin => UserType == "admin";

    public bool IsResident => UserType == "resident";
}
