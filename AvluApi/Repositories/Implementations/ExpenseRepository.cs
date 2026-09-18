using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;

namespace AvluApi.Repositories.Implementations;

public class ExpenseRepository : BaseRepository, IExpenseRepository
{
    public ExpenseRepository(IDbConnectionFactory dbFactory, ISiteContext siteContext)
        : base(dbFactory, siteContext) { }

    public async Task<IEnumerable<Expense>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, string? category, CancellationToken ct = default)
    {
        var sql = """
            SELECT * FROM dbo.Expenses
            WHERE SiteId = @SiteId AND IsDeleted = 0
            """;
        if (dateFrom.HasValue)              sql += " AND ExpenseDate >= @DateFrom";
        if (dateTo.HasValue)                sql += " AND ExpenseDate <= @DateTo";
        if (!string.IsNullOrEmpty(category)) sql += " AND Category = @Category";
        sql += " ORDER BY ExpenseDate DESC";

        using var conn = DbFactory.CreateConnection();
        return await conn.QueryAsync<Expense>(new CommandDefinition(
            sql, new { SiteId, DateFrom = dateFrom, DateTo = dateTo, Category = category },
            cancellationToken: ct));
    }

    public async Task<Expense?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Expense>(new CommandDefinition(
            "SELECT * FROM dbo.Expenses WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0",
            new { Id = id, SiteId }, cancellationToken: ct));
    }

    public async Task<long> CreateAsync(Expense expense, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.Expenses (SiteId, Category, Amount, ExpenseDate, Description, CreatedByUserId)
            OUTPUT INSERTED.Id
            VALUES (@SiteId, @Category, @Amount, @ExpenseDate, @Description, @CreatedByUserId)
            """;
        using var conn = DbFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql,
            new { SiteId, expense.Category, expense.Amount, expense.ExpenseDate, expense.Description, expense.CreatedByUserId },
            cancellationToken: ct));
    }

    public async Task UpdateAsync(Expense expense, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.Expenses
            SET Category = @Category, Amount = @Amount, ExpenseDate = @ExpenseDate, Description = @Description
            WHERE Id = @Id AND SiteId = @SiteId AND IsDeleted = 0
            """;
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql,
            new { expense.Category, expense.Amount, expense.ExpenseDate, expense.Description, expense.Id, SiteId },
            cancellationToken: ct));
    }

    public async Task SoftDeleteAsync(long id, CancellationToken ct = default)
    {
        using var conn = DbFactory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE dbo.Expenses SET IsDeleted = 1 WHERE Id = @Id AND SiteId = @SiteId",
            new { Id = id, SiteId }, cancellationToken: ct));
    }
}
