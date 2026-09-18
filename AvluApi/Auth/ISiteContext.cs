namespace AvluApi.Auth;

public interface ISiteContext
{
    long SiteId { get; }
    long UserId { get; }
    string Role { get; }
    string UserType { get; }
    bool IsAdmin { get; }
    bool IsResident { get; }
}
