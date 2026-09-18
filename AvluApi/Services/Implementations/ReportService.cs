using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.DTOs;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

file sealed class MonthAmountRow
{
    public int     Month  { get; set; }
    public decimal Amount { get; set; }
}

public class ReportService : IReportService
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ISiteContext         _siteContext;

    public ReportService(IDbConnectionFactory dbFactory, ISiteContext siteContext)
    {
        _dbFactory   = dbFactory;
        _siteContext = siteContext;
    }

    public async Task<MonthlyCollectionDto> GetMonthlyCollectionAsync(int year, int month, CancellationToken ct = default)
    {
        var period = year * 100 + month;
        using var conn = _dbFactory.CreateConnection();
        return await conn.QuerySingleAsync<MonthlyCollectionDto>(new CommandDefinition(
            """
            SELECT
                @Year  AS Year,
                @Month AS Month,
                ISNULL(SUM(Amount), 0)              AS TotalCharged,
                ISNULL(SUM(PaidAmount), 0)          AS TotalCollected,
                ISNULL(SUM(Amount - PaidAmount), 0) AS TotalPending,
                COUNT(DISTINCT PropertyId)          AS TotalProperties
            FROM dbo.DuesCharges
            WHERE SiteId = @SiteId AND Period = @Period AND IsDeleted = 0
            """,
            new { SiteId = _siteContext.SiteId, Year = year, Month = month, Period = period },
            cancellationToken: ct));
    }

    public async Task<DebtAgingDto> GetDebtAgingAsync(CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        return await conn.QuerySingleAsync<DebtAgingDto>(new CommandDefinition(
            """
            SELECT
                ISNULL(SUM(CASE WHEN DATEDIFF(DAY, DueDate, SYSUTCDATETIME()) BETWEEN 0  AND 30 AND Status != 'Paid' THEN Amount - PaidAmount ELSE 0 END), 0) AS Days0To30,
                ISNULL(SUM(CASE WHEN DATEDIFF(DAY, DueDate, SYSUTCDATETIME()) BETWEEN 31 AND 60 AND Status != 'Paid' THEN Amount - PaidAmount ELSE 0 END), 0) AS Days31To60,
                ISNULL(SUM(CASE WHEN DATEDIFF(DAY, DueDate, SYSUTCDATETIME()) BETWEEN 61 AND 90 AND Status != 'Paid' THEN Amount - PaidAmount ELSE 0 END), 0) AS Days61To90,
                ISNULL(SUM(CASE WHEN DATEDIFF(DAY, DueDate, SYSUTCDATETIME()) > 90        AND Status != 'Paid' THEN Amount - PaidAmount ELSE 0 END), 0) AS DaysOver90
            FROM dbo.DuesCharges
            WHERE SiteId = @SiteId AND DueDate < SYSUTCDATETIME() AND IsDeleted = 0
            """,
            new { SiteId = _siteContext.SiteId }, cancellationToken: ct));
    }

    public async Task<IEnumerable<PropertySummaryDto>> GetPropertySummaryAsync(CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        return await conn.QueryAsync<PropertySummaryDto>(new CommandDefinition(
            """
            SELECT
                p.Id    AS PropertyId,
                p.Block AS Block,
                p.UnitNo AS UnitNo,
                ISNULL(SUM(c.Amount), 0)     AS TotalDebt,
                ISNULL(SUM(c.PaidAmount), 0) AS TotalPaid
            FROM dbo.Properties p
            LEFT JOIN dbo.DuesCharges c
                ON p.Id = c.PropertyId AND c.SiteId = p.SiteId AND c.IsDeleted = 0
            WHERE p.SiteId = @SiteId AND p.IsActive = 1 AND p.IsDeleted = 0
            GROUP BY p.Id, p.Block, p.UnitNo
            ORDER BY p.Block, p.UnitNo
            """,
            new { SiteId = _siteContext.SiteId }, cancellationToken: ct));
    }

    public async Task<IEnumerable<OverdueChargeDto>> GetOverdueAsync(CancellationToken ct = default)
    {
        using var conn = _dbFactory.CreateConnection();
        return await conn.QueryAsync<OverdueChargeDto>(new CommandDefinition(
            """
            SELECT
                c.Id         AS ChargeId,
                c.PropertyId AS PropertyId,
                p.Block      AS Block,
                p.UnitNo     AS UnitNo,
                c.Amount,
                c.PaidAmount,
                c.DueDate,
                DATEDIFF(DAY, c.DueDate, SYSUTCDATETIME()) AS DaysOverdue,
                c.Status
            FROM dbo.DuesCharges c
            JOIN dbo.Properties p ON c.PropertyId = p.Id
            WHERE c.SiteId = @SiteId
              AND c.Status != 'Paid'
              AND c.DueDate < SYSUTCDATETIME()
              AND c.IsDeleted = 0
            ORDER BY c.DueDate ASC
            """,
            new { SiteId = _siteContext.SiteId }, cancellationToken: ct));
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var siteId = _siteContext.SiteId;
        var now    = DateTime.UtcNow;
        var period = now.Year * 100 + now.Month;

        using var conn = _dbFactory.CreateConnection();

        var dto = new DashboardDto();

        // Active property count
        dto.TotalActiveProperties = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM dbo.Properties WHERE SiteId = @SiteId AND IsActive = 1 AND IsDeleted = 0",
            new { SiteId = siteId }, cancellationToken: ct));

        // Total pending dues (all unpaid/partial charges)
        dto.TotalPendingDues = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition(
            """
            SELECT ISNULL(SUM(Amount - PaidAmount), 0)
            FROM dbo.DuesCharges
            WHERE SiteId = @SiteId AND Status != 'Paid' AND IsDeleted = 0
            """,
            new { SiteId = siteId }, cancellationToken: ct));

        // Current month charged & collected
        var monthly = await conn.QuerySingleAsync<MonthlyCollectionDto>(new CommandDefinition(
            """
            SELECT
                ISNULL(SUM(Amount), 0)     AS TotalCharged,
                ISNULL(SUM(PaidAmount), 0) AS TotalCollected
            FROM dbo.DuesCharges
            WHERE SiteId = @SiteId AND Period = @Period AND IsDeleted = 0
            """,
            new { SiteId = siteId, Period = period }, cancellationToken: ct));
        dto.CurrentMonthCharged   = monthly.TotalCharged;
        dto.CurrentMonthCollected = monthly.TotalCollected;

        // Overdue count
        dto.OverdueCount = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            """
            SELECT COUNT(*)
            FROM dbo.DuesCharges
            WHERE SiteId = @SiteId AND Status != 'Paid' AND DueDate < SYSUTCDATETIME() AND IsDeleted = 0
            """,
            new { SiteId = siteId }, cancellationToken: ct));

        // Total expenses year-to-date
        dto.TotalExpensesYtd = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition(
            """
            SELECT ISNULL(SUM(Amount), 0)
            FROM dbo.Expenses
            WHERE SiteId = @SiteId AND YEAR(ExpenseDate) = @Year AND IsDeleted = 0
            """,
            new { SiteId = siteId, Year = now.Year }, cancellationToken: ct));

        // Total credit balance across all properties
        dto.TotalCreditBalance = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition(
            "SELECT ISNULL(SUM(CreditBalance), 0) FROM dbo.PropertyBalances WHERE SiteId = @SiteId",
            new { SiteId = siteId }, cancellationToken: ct));

        return dto;
    }

    public async Task<IEnumerable<IncomeExpenseMonthDto>> GetIncomeExpenseAsync(int year, CancellationToken ct = default)
    {
        var siteId = _siteContext.SiteId;
        using var conn = _dbFactory.CreateConnection();

        // Dues income per month (payments on dues charges)
        var duesIncome = await conn.QueryAsync<MonthAmountRow>(new CommandDefinition(
            """
            SELECT MONTH(p.PaidAt) AS Month, ISNULL(SUM(p.Amount), 0) AS Amount
            FROM dbo.Payments p
            WHERE p.SiteId = @SiteId AND p.TargetType = 'Dues'
              AND YEAR(p.PaidAt) = @Year AND p.IsDeleted = 0
            GROUP BY MONTH(p.PaidAt)
            """,
            new { SiteId = siteId, Year = year }, cancellationToken: ct));

        // Extra payment income per month
        var extraIncome = await conn.QueryAsync<MonthAmountRow>(new CommandDefinition(
            """
            SELECT MONTH(p.PaidAt) AS Month, ISNULL(SUM(p.Amount), 0) AS Amount
            FROM dbo.Payments p
            WHERE p.SiteId = @SiteId AND p.TargetType = 'Extra'
              AND YEAR(p.PaidAt) = @Year AND p.IsDeleted = 0
            GROUP BY MONTH(p.PaidAt)
            """,
            new { SiteId = siteId, Year = year }, cancellationToken: ct));

        // Expenses per month
        var expenses = await conn.QueryAsync<MonthAmountRow>(new CommandDefinition(
            """
            SELECT MONTH(ExpenseDate) AS Month, ISNULL(SUM(Amount), 0) AS Amount
            FROM dbo.Expenses
            WHERE SiteId = @SiteId AND YEAR(ExpenseDate) = @Year AND IsDeleted = 0
            GROUP BY MONTH(ExpenseDate)
            """,
            new { SiteId = siteId, Year = year }, cancellationToken: ct));

        var duesMap  = duesIncome.ToDictionary(x => x.Month, x => x.Amount);
        var extraMap = extraIncome.ToDictionary(x => x.Month, x => x.Amount);
        var expMap   = expenses.ToDictionary(x => x.Month, x => x.Amount);

        return Enumerable.Range(1, 12).Select(m => new IncomeExpenseMonthDto
        {
            Year        = year,
            Month       = m,
            IncomeDues  = duesMap.GetValueOrDefault(m),
            IncomeExtra = extraMap.GetValueOrDefault(m),
            Expense     = expMap.GetValueOrDefault(m)
        });
    }
}
