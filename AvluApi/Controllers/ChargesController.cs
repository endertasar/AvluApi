using AvluApi.Models.DTOs;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/charges")]
[Authorize]
public class ChargesController : ControllerBase
{
    private readonly IChargeService _chargeService;

    public ChargesController(IChargeService chargeService)
    {
        _chargeService = chargeService;
    }

    /// <summary>Borç listesi — Manager/SuperAdmin (filtre: propertyId, status, period)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DuesChargeDto>>>> GetAll(
        [FromQuery] long? propertyId,
        [FromQuery] string? status,
        [FromQuery] int? period)
    {
        var charges = await _chargeService.GetAllAsync(propertyId, status, period);
        return Ok(ApiResponse<IEnumerable<DuesChargeDto>>.Ok(charges));
    }

    /// <summary>Borç detayı — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<DuesChargeDto>>> GetById(long id)
    {
        var charge = await _chargeService.GetByIdAsync(id);
        return Ok(ApiResponse<DuesChargeDto>.Ok(charge));
    }

    /// <summary>Özet (dashboard) — Manager/SuperAdmin</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<ChargeSummaryDto>>> GetSummary()
    {
        var summary = await _chargeService.GetSummaryAsync();
        return Ok(ApiResponse<ChargeSummaryDto>.Ok(summary));
    }
}
