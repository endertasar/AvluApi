using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/extra-payments")]
[Authorize]
public class ExtraPaymentsController : ControllerBase
{
    private readonly IExtraPaymentService _service;

    public ExtraPaymentsController(IExtraPaymentService service)
    {
        _service = service;
    }

    /// <summary>Ek ödeme listesi — Manager/SuperAdmin</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExtraPaymentDto>>>> GetAll(CancellationToken ct)
    {
        var list = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<ExtraPaymentDto>>.Ok(list));
    }

    /// <summary>Ek ödeme detayı ve taksit özeti — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<ExtraPaymentDetailDto>>> GetById(long id, CancellationToken ct)
    {
        var detail = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ExtraPaymentDetailDto>.Ok(detail));
    }

    /// <summary>Ek ödeme tanımla ve tahakkukları üret — Manager/SuperAdmin</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExtraPaymentDto>>> Create(
        [FromBody] CreateExtraPaymentRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ExtraPaymentDto>.Ok(result, "Ek ödeme oluşturuldu."));
    }

    /// <summary>Ek ödeme mülk tahakkukları — Manager/SuperAdmin (filtre: propertyId, status)</summary>
    [HttpGet("{id:long}/charges")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExtraPaymentChargeDto>>>> GetCharges(
        long id,
        [FromQuery] long? propertyId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var charges = await _service.GetChargesAsync(id, propertyId, status, ct);
        return Ok(ApiResponse<IEnumerable<ExtraPaymentChargeDto>>.Ok(charges));
    }
}
