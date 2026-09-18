using AvluApi.Auth;
using AvluApi.Infrastructure.Database;

namespace AvluApi.Repositories;

public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory DbFactory;
    protected readonly ISiteContext SiteContext;
    protected long SiteId => SiteContext.SiteId;

    protected BaseRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
    {
        DbFactory = dbFactory;
        SiteContext = siteContext;
    }
}
