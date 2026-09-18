using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class PropertyBalanceRepository : BaseRepository, IPropertyBalanceRepository
{
    public PropertyBalanceRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<decimal> GetCreditBalanceAsync(long propertyId, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<decimal?>(
            "SELECT CreditBalance FROM dbo.PropertyBalances WHERE PropertyId = @PropertyId AND SiteId = @SiteId",
            new { PropertyId = propertyId, SiteId }) ?? 0m;
    }

    public async Task<IEnumerable<BalanceTransaction>> GetTransactionsAsync(long propertyId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, SiteId, PropertyId, Direction, Amount, Source, SourceId, Note, CreatedAt
            FROM dbo.BalanceTransactions
            WHERE PropertyId = @PropertyId AND SiteId = @SiteId
            ORDER BY CreatedAt DESC;
            """;
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<BalanceTransaction>(sql, new { PropertyId = propertyId, SiteId });
    }
}
