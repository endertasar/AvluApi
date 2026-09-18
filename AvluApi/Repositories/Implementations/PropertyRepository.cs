using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class PropertyRepository : BaseRepository, IPropertyRepository
{
    public PropertyRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<Property>(
            "SELECT * FROM Properties WHERE SiteId = @SiteId AND IsDeleted = 0 ORDER BY Block, UnitNo",
            new { SiteId });
    }

    public async Task<Property?> GetByIdAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Property>(
            "SELECT * FROM Properties WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId });
    }

    public async Task<long> CreateAsync(Property property)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QuerySingleAsync<long>(
            @"INSERT INTO Properties (SiteId, Block, UnitNo, Type, OwnerName, OwnerPhone, ResidentName, ResidentPhone, DuesResponsible)
              OUTPUT INSERTED.Id
              VALUES (@SiteId, @Block, @UnitNo, @Type, @OwnerName, @OwnerPhone, @ResidentName, @ResidentPhone, @DuesResponsible);",
            new { SiteId, property.Block, property.UnitNo, property.Type, property.OwnerName,
                  property.OwnerPhone, property.ResidentName, property.ResidentPhone, property.DuesResponsible });
    }

    public async Task UpdateAsync(Property property)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE Properties SET Block = @Block, UnitNo = @UnitNo, Type = @Type,
              OwnerName = @OwnerName, OwnerPhone = @OwnerPhone,
              ResidentName = @ResidentName, ResidentPhone = @ResidentPhone,
              DuesResponsible = @DuesResponsible, IsActive = @IsActive
              WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { property.Block, property.UnitNo, property.Type, property.OwnerName,
                  property.OwnerPhone, property.ResidentName, property.ResidentPhone,
                  property.DuesResponsible, property.IsActive, property.Id, SiteId });
    }

    public async Task SoftDeleteAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Properties SET IsDeleted = 1 WHERE Id = @Id AND SiteId = @SiteId",
            new { Id = id, SiteId });
    }
}
