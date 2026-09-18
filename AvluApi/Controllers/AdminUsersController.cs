using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = "OwnerOnly")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _service;

    public AdminUsersController(IAdminUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AdminUserInfo>>>> GetAll(CancellationToken ct)
    {
        var users = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<AdminUserInfo>>.Ok(users));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<AdminUserInfo>>> GetById(long id, CancellationToken ct)
    {
        var user = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<AdminUserInfo>.Ok(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminUserInfo>>> Create(
        [FromBody] CreateAdminUserRequest request, CancellationToken ct)
    {
        var user = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiResponse<AdminUserInfo>.Ok(user, "Kullanıcı oluşturuldu."));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<AdminUserInfo>>> Update(
        long id, [FromBody] UpdateAdminUserRequest request, CancellationToken ct)
    {
        var user = await _service.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<AdminUserInfo>.Ok(user, "Kullanıcı güncellendi."));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.Ok("Kullanıcı silindi."));
    }
}
