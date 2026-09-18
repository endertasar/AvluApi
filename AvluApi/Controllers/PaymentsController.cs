using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Tahsilat listesi — Manager/SuperAdmin (filtre: propertyId, dateFrom, dateTo)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetAll(
        [FromQuery] long? propertyId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var payments = await _paymentService.GetAllAsync(propertyId, dateFrom, dateTo, ct);
        return Ok(ApiResponse<IEnumerable<PaymentDto>>.Ok(payments));
    }

    /// <summary>Tahsilat detayı — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(long id, CancellationToken ct)
    {
        var payment = await _paymentService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    /// <summary>Tahsilat kaydet (kısmi, kredi, fazla ödeme destekli) — Manager/SuperAdmin</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreatePaymentResponse>>> Create(
        [FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.CreateAsync(request, ct);
        return Ok(ApiResponse<CreatePaymentResponse>.Ok(result, "Tahsilat kaydedildi."));
    }

    /// <summary>Tahsilat iptal — Manager/SuperAdmin</summary>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Cancel(long id, CancellationToken ct)
    {
        await _paymentService.CancelAsync(id, ct);
        return Ok(ApiResponse<string>.Ok("Tahsilat iptal edildi."));
    }
}
