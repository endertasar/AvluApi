using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class ExtraPaymentRepository : BaseRepository, IExtraPaymentRepository
{
    public ExtraPaymentRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<ExtraPayment>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<ExtraPayment>(new CommandDefinition(
            "SELECT * FROM dbo.ExtraPayments WHERE SiteId = @SiteId AND IsDeleted = 0 ORDER BY CreatedAt DESC",
            new { SiteId }, cancellationToken: ct));
    }

    public async Task<ExtraPayment?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ExtraPayment>(new CommandDefinition(
            "SELECT * FROM dbo.ExtraPayments WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId }, cancellationToken: ct));
    }

    public async Task<long> CreateAsync(ExtraPayment ep, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.ExtraPayments (SiteId, Name, AmountPerProperty, TotalAmount, InstallmentCount)
            OUTPUT INSERTED.Id
            VALUES (@SiteId, @Name, @AmountPerProperty, @TotalAmount, @InstallmentCount)
            """;
        using var conn = DbFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql,
            new { SiteId, ep.Name, ep.AmountPerProperty, ep.TotalAmount, ep.InstallmentCount },
            cancellationToken: ct));
    }

    public async Task<IEnumerable<ExtraPaymentCharge>> GetChargesAsync(
        long extraPaymentId, long? propertyId, string? status, CancellationToken ct = default)
    {
        var sql = """
            SELECT * FROM dbo.ExtraPaymentCharges
            WHERE ExtraPaymentId = @ExtraPaymentId AND SiteId = @SiteId AND IsDeleted = 0
            """;
        if (propertyId.HasValue) sql += " AND PropertyId = @PropertyId";
        if (!string.IsNullOrEmpty(status)) sql += " AND Status = @Status";
        sql += " ORDER BY PropertyId, InstallmentNo";

        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<ExtraPaymentCharge>(new CommandDefinition(
            sql, new { ExtraPaymentId = extraPaymentId, SiteId, PropertyId = propertyId, Status = status },
            cancellationToken: ct));
    }

    public async Task<ExtraPaymentCharge?> GetChargeByIdAsync(long id, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ExtraPaymentCharge>(new CommandDefinition(
            "SELECT * FROM dbo.ExtraPaymentCharges WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId }, cancellationToken: ct));
    }

    public async Task<int> BulkInsertChargesAsync(IEnumerable<ExtraPaymentCharge> charges, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.ExtraPaymentCharges (SiteId, ExtraPaymentId, PropertyId, InstallmentNo, Amount)
            VALUES (@SiteId, @ExtraPaymentId, @PropertyId, @InstallmentNo, @Amount)
            """;
        var rows = charges.Select(c => new
        {
            SiteId         = SiteId,
            c.ExtraPaymentId,
            c.PropertyId,
            c.InstallmentNo,
            c.Amount
        }).ToList();
        using var conn = DbFactory.CreateConnection();
        return await conn.ExecuteAsync(new CommandDefinition(sql, rows, cancellationToken: ct));
    }

    public async Task UpdateChargeCountAsync(long id, int count, decimal totalAmount, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE dbo.ExtraPayments SET TotalAmount = @TotalAmount WHERE Id = @Id AND SiteId = @SiteId",
            new { Id = id, TotalAmount = totalAmount, SiteId }, cancellationToken: ct));
    }
}
