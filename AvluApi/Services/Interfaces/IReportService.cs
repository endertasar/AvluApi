using AvluApi.Models.DTOs;

namespace AvluApi.Services.Interfaces;

public interface IReportService
{
    Task<MonthlyCollectionDto>          GetMonthlyCollectionAsync(int year, int month, CancellationToken ct = default);
    Task<DebtAgingDto>                  GetDebtAgingAsync(CancellationToken ct = default);
    Task<IEnumerable<PropertySummaryDto>> GetPropertySummaryAsync(CancellationToken ct = default);
    Task<IEnumerable<OverdueChargeDto>> GetOverdueAsync(CancellationToken ct = default);
    Task<DashboardDto>                  GetDashboardAsync(CancellationToken ct = default);
    Task<IEnumerable<IncomeExpenseMonthDto>> GetIncomeExpenseAsync(int year, CancellationToken ct = default);
}
