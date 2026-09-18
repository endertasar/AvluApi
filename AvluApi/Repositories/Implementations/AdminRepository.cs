using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class AdminRepository : BaseRepository, IAdminRepository
{
    public AdminRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<AdminUser?> GetByIdAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<AdminUser>(
            "SELECT * FROM AdminUsers WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id });
    }

    public async Task<IEnumerable<AdminUser>> GetBySiteAsync()
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<AdminUser>(
            "SELECT * FROM AdminUsers WHERE SiteId = @SiteId AND IsDeleted = 0 ORDER BY Username",
            new { SiteId });
    }

    public async Task<long> CreateAsync(AdminUser user)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QuerySingleAsync<long>(
            @"INSERT INTO AdminUsers (SiteId, Username, PasswordHash, FullName, Role)
              OUTPUT INSERTED.Id
              VALUES (@SiteId, @Username, @PasswordHash, @FullName, @Role);",
            new { user.SiteId, user.Username, user.PasswordHash, user.FullName, user.Role });
    }

    public async Task UpdateAsync(long id, string? fullName, string role, bool isActive)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE AdminUsers SET FullName = @FullName, Role = @Role, IsActive = @IsActive WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId, FullName = fullName, Role = role, IsActive = isActive });
    }

    public async Task SoftDeleteAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE AdminUsers SET IsDeleted = 1 WHERE Id = @Id AND SiteId = @SiteId",
            new { Id = id, SiteId });
    }
}
