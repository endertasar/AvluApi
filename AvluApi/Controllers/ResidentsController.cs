using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/residents")]
[Authorize]
public class ResidentsController : ControllerBase
{
    private readonly IResidentService _residentService;

    public ResidentsController(IResidentService residentService)
    {
        _residentService = residentService;
    }

    /// <summary>Sakin detayı — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<ResidentDto>>> GetById(long id)
    {
        var resident = await _residentService.GetByIdAsync(id);
        return Ok(ApiResponse<ResidentDto>.Ok(resident));
    }

    /// <summary>Yeni sakin kaydı — Manager/SuperAdmin</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ResidentDto>>> Create([FromBody] CreateResidentRequest request)
    {
        var resident = await _residentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = resident.Id }, ApiResponse<ResidentDto>.Ok(resident, "Sakin eklendi."));
    }

    /// <summary>Sakin güncelle — Manager/SuperAdmin</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(long id, [FromBody] UpdateResidentRequest request)
    {
        await _residentService.UpdateAsync(id, request);
        return Ok(ApiResponse<string>.Ok("Sakin güncellendi."));
    }

    /// <summary>Sakin sil (soft delete) — Manager/SuperAdmin</summary>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id)
    {
        await _residentService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Sakin silindi."));
    }
}
