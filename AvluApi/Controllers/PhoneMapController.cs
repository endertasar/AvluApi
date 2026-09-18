using AvluApi.Auth;
using AvluApi.Models.DTOs;
using AvluApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/resident")]
[Authorize(Policy = "ResidentOnly")]
public class ResidentPropertiesController : ControllerBase
{
    private readonly IPhoneMapRepository _phoneMapRepo;
    private readonly IResidentRepository _residentRepo;

    public ResidentPropertiesController(IPhoneMapRepository phoneMapRepo, IResidentRepository residentRepo)
    {
        _phoneMapRepo = phoneMapRepo;
        _residentRepo = residentRepo;
    }

    [HttpGet("properties")]
    public async Task<ActionResult<IEnumerable<PropertyDto>>> MyProperties()
    {
        var residentId = User.GetUserId();
        var resident = await _residentRepo.GetByIdAsync(residentId);
        if (resident == null) return Unauthorized();

        var properties = await _phoneMapRepo.GetPropertiesByPhoneAsync(resident.Phone);
        var result = properties.Select(p => new PropertyDto
        {
            Id = p.Id,
            Block = p.Block,
            UnitNo = p.UnitNo,
            Type = p.Type,
            OwnerName = p.OwnerName,
            OwnerPhone = p.OwnerPhone,
            ResidentName = p.ResidentName,
            ResidentPhone = p.ResidentPhone,
            DuesResponsible = p.DuesResponsible,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        });

        return Ok(result);
    }
}
