using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/dues")]
[Authorize]
public class DuesController : ControllerBase
{
    private readonly IDuesService _duesService;

    public DuesController(IDuesService duesService)
    {
        _duesService = duesService;
    }

    /// <summary>Aidat tanımları listesi — Manager/SuperAdmin</summary>
    [HttpGet("definitions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DuesDefinition>>>> GetDefinitions()
    {
        var defs = await _duesService.GetDefinitionsAsync();
        return Ok(ApiResponse<IEnumerable<DuesDefinition>>.Ok(defs));
    }

    /// <summary>Yeni aidat tanımı — Manager/SuperAdmin</summary>
    [HttpPost("definitions")]
    public async Task<ActionResult<ApiResponse<DuesDefinition>>> CreateDefinition([FromBody] CreateDuesDefinitionRequest request)
    {
        var def = await _duesService.CreateDefinitionAsync(request);
        return Ok(ApiResponse<DuesDefinition>.Ok(def, "Aidat tanımı oluşturuldu."));
    }

    /// <summary>Aidat tanımı sil (soft delete) — Manager/SuperAdmin</summary>
    [HttpDelete("definitions/{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteDefinition(long id)
    {
        await _duesService.SoftDeleteDefinitionAsync(id);
        return Ok(ApiResponse<string>.Ok("Aidat tanımı silindi."));
    }

    /// <summary>Tekil borç ekle — Manager/SuperAdmin</summary>
    [HttpPost("charges")]
    public async Task<ActionResult<ApiResponse<DuesChargeDto>>> CreateCharge([FromBody] CreateChargeRequest request)
    {
        var charge = await _duesService.CreateChargeAsync(request);
        return Ok(ApiResponse<DuesChargeDto>.Ok(charge, "Borç kaydedildi."));
    }

    /// <summary>Belirtilen dönem için tahakkuk üret (idempotent) — Manager/SuperAdmin</summary>
    [HttpPost("charges/generate")]
    public async Task<ActionResult<ApiResponse<GenerateChargesResultDto>>> GenerateCharges(
        [FromBody] GenerateChargesRequest request, CancellationToken ct)
    {
        var result = await _duesService.GenerateChargesAsync(request.Period, ct);
        return Ok(ApiResponse<GenerateChargesResultDto>.Ok(result,
            $"Dönem {request.Period}: {result.Generated} oluşturuldu, {result.Skipped} atlandı, {result.Missing} tanımsız."));
    }
}
