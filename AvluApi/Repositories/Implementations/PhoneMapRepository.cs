using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class PhoneMapRepository : BaseRepository, IPhoneMapRepository
{
    public PhoneMapRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<Property>> GetPropertiesByPhoneAsync(string phone)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<Property>(
            @"SELECT * FROM Properties
              WHERE (OwnerPhone = @Phone OR ResidentPhone = @Phone)
              AND IsDeleted = 0 AND IsActive = 1",
            new { Phone = phone });
    }
}
