using AvluApi.Models.DTOs;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Dashboard özeti — aktif mülk, bekleyen borç, tahsilat oranı, gider, kredi bakiyesi</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> Dashboard(CancellationToken ct)
    {
        var dto = await _reportService.GetDashboardAsync(ct);
        return Ok(ApiResponse<DashboardDto>.Ok(dto));
    }

    /// <summary>Aylık tahsilat özeti — Manager/SuperAdmin</summary>
    [HttpGet("monthly-collection")]
    public async Task<ActionResult<ApiResponse<MonthlyCollectionDto>>> MonthlyCollection(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        var report = await _reportService.GetMonthlyCollectionAsync(year, month, ct);
        return Ok(ApiResponse<MonthlyCollectionDto>.Ok(report));
    }

    /// <summary>Borç yaşlandırma raporu (0-30, 31-60, 61-90, 90+) — Manager/SuperAdmin</summary>
    [HttpGet("debt-aging")]
    public async Task<ActionResult<ApiResponse<DebtAgingDto>>> DebtAging(CancellationToken ct)
    {
        var report = await _reportService.GetDebtAgingAsync(ct);
        return Ok(ApiResponse<DebtAgingDto>.Ok(report));
    }

    /// <summary>Mülk bazlı borç/tahsilat özeti — Manager/SuperAdmin</summary>
    [HttpGet("property-summary")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PropertySummaryDto>>>> PropertySummary(CancellationToken ct)
    {
        var report = await _reportService.GetPropertySummaryAsync(ct);
        return Ok(ApiResponse<IEnumerable<PropertySummaryDto>>.Ok(report));
    }

    /// <summary>Vadesi geçmiş borçlar — Manager/SuperAdmin</summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OverdueChargeDto>>>> Overdue(CancellationToken ct)
    {
        var report = await _reportService.GetOverdueAsync(ct);
        return Ok(ApiResponse<IEnumerable<OverdueChargeDto>>.Ok(report));
    }

    /// <summary>Yıllık gelir/gider karşılaştırması (12 ay) — Manager/SuperAdmin</summary>
    [HttpGet("income-expense")]
    public async Task<ActionResult<ApiResponse<IEnumerable<IncomeExpenseMonthDto>>>> IncomeExpense(
        [FromQuery] int year,
        CancellationToken ct)
    {
        var report = await _reportService.GetIncomeExpenseAsync(year, ct);
        return Ok(ApiResponse<IEnumerable<IncomeExpenseMonthDto>>.Ok(report));
    }
}
