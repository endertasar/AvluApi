using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class DuesRepository : BaseRepository, IDuesRepository
{
    public DuesRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<DuesDefinition>> GetAllAsync()
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<DuesDefinition>(
            "SELECT * FROM DuesDefinitions WHERE SiteId = @SiteId AND IsDeleted = 0 ORDER BY PropertyType, EffectiveFrom DESC",
            new { SiteId });
    }

    public async Task<DuesDefinition?> GetByIdAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DuesDefinition>(
            "SELECT * FROM DuesDefinitions WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId });
    }

    public async Task<DuesDefinition?> GetActiveForTypeAndPeriodAsync(long siteId, string propertyType, int period)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DuesDefinition>(
            @"SELECT TOP 1 * FROM DuesDefinitions
              WHERE SiteId = @SiteId AND PropertyType = @PropertyType AND EffectiveFrom <= @Period AND IsDeleted = 0
              ORDER BY EffectiveFrom DESC",
            new { SiteId = siteId, PropertyType = propertyType, Period = period });
    }

    public async Task<long> CreateAsync(DuesDefinition def)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QuerySingleAsync<long>(
            @"INSERT INTO DuesDefinitions (SiteId, PropertyType, Amount, EffectiveFrom, Description)
              OUTPUT INSERTED.Id
              VALUES (@SiteId, @PropertyType, @Amount, @EffectiveFrom, @Description);",
            new { SiteId, def.PropertyType, def.Amount, def.EffectiveFrom, def.Description });
    }

    public async Task SoftDeleteAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE DuesDefinitions SET IsDeleted = 1 WHERE Id = @Id AND SiteId = @SiteId",
            new { Id = id, SiteId });
    }
}
