using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class ChargeRepository : BaseRepository, IChargeRepository
{
    public ChargeRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<DuesCharge>> GetAllAsync(long? propertyId, string? status, int? period)
    {
        using var conn = DbFactory.CreateConnection();
        var sql = "SELECT * FROM DuesCharges WHERE SiteId = @SiteId AND IsDeleted = 0";
        if (propertyId.HasValue) sql += " AND PropertyId = @PropertyId";
        if (!string.IsNullOrEmpty(status)) sql += " AND Status = @Status";
        if (period.HasValue) sql += " AND Period = @Period";
        sql += " ORDER BY Period DESC, PropertyId";

        return await conn.QueryAsync<DuesCharge>(sql,
            new { SiteId, PropertyId = propertyId, Status = status, Period = period });
    }

    public async Task<DuesCharge?> GetByIdAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DuesCharge>(
            "SELECT * FROM DuesCharges WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId });
    }

    public async Task<ChargeSummaryDto> GetSummaryAsync()
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QuerySingleAsync<ChargeSummaryDto>(
            @"SELECT
                ISNULL(SUM(Amount), 0) AS TotalDebt,
                ISNULL(SUM(PaidAmount), 0) AS TotalCollected,
                ISNULL(SUM(CASE WHEN Status != 'Paid' AND DueDate < SYSUTCDATETIME() THEN Amount - PaidAmount ELSE 0 END), 0) AS TotalOverdue,
                COUNT(CASE WHEN Status != 'Paid' AND DueDate < SYSUTCDATETIME() THEN 1 END) AS OverdueCount
              FROM DuesCharges WHERE SiteId = @SiteId AND IsDeleted = 0",
            new { SiteId });
    }

    public async Task<long> CreateAsync(DuesCharge charge)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QuerySingleAsync<long>(
            @"INSERT INTO DuesCharges (SiteId, PropertyId, Period, Amount, DueDate)
              OUTPUT INSERTED.Id
              VALUES (@SiteId, @PropertyId, @Period, @Amount, @DueDate);",
            new { SiteId, charge.PropertyId, charge.Period, charge.Amount, charge.DueDate });
    }

    public async Task UpdatePaidAmountAsync(long id, decimal paidAmount, string status)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE DuesCharges SET PaidAmount = @PaidAmount, Status = @Status WHERE Id = @Id AND SiteId = @SiteId",
            new { PaidAmount = paidAmount, Status = status, Id = id, SiteId });
    }
}
