using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class ResidentRepository : BaseRepository, IResidentRepository
{
    public ResidentRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<Resident?> GetByIdAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Resident>(
            "SELECT * FROM Residents WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id });
    }

    public async Task<Resident?> GetByPhoneAsync(string phone)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Resident>(
            "SELECT * FROM Residents WHERE Phone = @Phone AND IsDeleted = 0",
            new { Phone = phone });
    }

    public async Task<long> CreateAsync(Resident resident)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QuerySingleAsync<long>(
            @"INSERT INTO Residents (Phone, PasswordHash, FullName)
              OUTPUT INSERTED.Id
              VALUES (@Phone, @PasswordHash, @FullName);",
            new { resident.Phone, resident.PasswordHash, resident.FullName });
    }

    public async Task UpdateAsync(Resident resident)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Residents SET FullName = @FullName, OneSignalUserId = @OneSignalUserId WHERE Id = @Id AND IsDeleted = 0",
            new { resident.FullName, resident.OneSignalUserId, resident.Id });
    }

    public async Task SoftDeleteAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Residents SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = id });
    }
}
