using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/properties")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    /// <summary>Mülk listesi — Manager/SuperAdmin (SiteId otomatik filtre)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PropertyDto>>>> GetAll(CancellationToken ct)
    {
        var props = await _propertyService.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<PropertyDto>>.Ok(props));
    }

    /// <summary>Mülk detayı — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PropertyDto>>> GetById(long id, CancellationToken ct)
    {
        var prop = await _propertyService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PropertyDto>.Ok(prop));
    }

    /// <summary>Yeni mülk tanımla — Manager/SuperAdmin</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PropertyDto>>> Create(
        [FromBody] CreatePropertyRequest request, CancellationToken ct)
    {
        var prop = await _propertyService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = prop.Id },
            ApiResponse<PropertyDto>.Ok(prop, "Mülk oluşturuldu."));
    }

    /// <summary>Mülk güncelle — Manager/SuperAdmin</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<PropertyDto>>> Update(
        long id, [FromBody] UpdatePropertyRequest request, CancellationToken ct)
    {
        var prop = await _propertyService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<PropertyDto>.Ok(prop, "Mülk güncellendi."));
    }

    /// <summary>Mülk sil (soft delete) — Manager/SuperAdmin</summary>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await _propertyService.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.Ok("Mülk silindi."));
    }

    /// <summary>Mülk alacak bakiyesi ve bakiye hareketleri — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}/balance")]
    public async Task<ActionResult<ApiResponse<PropertyBalanceDto>>> GetBalance(long id, CancellationToken ct)
    {
        var balance = await _propertyService.GetBalanceAsync(id, ct);
        return Ok(ApiResponse<PropertyBalanceDto>.Ok(balance));
    }
}
