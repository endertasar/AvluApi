using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class PaymentRepository : BaseRepository, IPaymentRepository
{
    public PaymentRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<Payment>> GetAllAsync(long? propertyId, DateTime? dateFrom, DateTime? dateTo)
    {
        using var conn = DbFactory.CreateConnection();
        var sql = "SELECT * FROM Payments WHERE SiteId = @SiteId AND IsDeleted = 0";
        if (propertyId.HasValue) sql += " AND PropertyId = @PropertyId";
        if (dateFrom.HasValue) sql += " AND PaidAt >= @DateFrom";
        if (dateTo.HasValue) sql += " AND PaidAt <= @DateTo";
        sql += " ORDER BY PaidAt DESC";

        return await conn.QueryAsync<Payment>(sql,
            new { SiteId, PropertyId = propertyId, DateFrom = dateFrom, DateTo = dateTo });
    }

    public async Task<Payment?> GetByIdAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Payment>(
            "SELECT * FROM Payments WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId });
    }

    public async Task<IEnumerable<Payment>> GetByTargetAsync(string targetType, long targetId)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<Payment>(
            "SELECT * FROM Payments WHERE SiteId = @SiteId AND TargetType = @TargetType AND TargetId = @TargetId AND IsDeleted = 0 ORDER BY PaidAt DESC",
            new { SiteId, TargetType = targetType, TargetId = targetId });
    }

    public async Task<long> CreateAsync(Payment payment)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QuerySingleAsync<long>(
            @"INSERT INTO Payments (SiteId, PropertyId, TargetType, TargetId, Amount, Method, Note, CollectedByUserId)
              OUTPUT INSERTED.Id
              VALUES (@SiteId, @PropertyId, @TargetType, @TargetId, @Amount, @Method, @Note, @CollectedByUserId);",
            new { SiteId, payment.PropertyId, payment.TargetType, payment.TargetId,
                  payment.Amount, payment.Method, payment.Note, payment.CollectedByUserId });
    }

    public async Task SoftDeleteAsync(long id)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Payments SET IsDeleted = 1 WHERE Id = @Id AND SiteId = @SiteId",
            new { Id = id, SiteId });
    }
}
