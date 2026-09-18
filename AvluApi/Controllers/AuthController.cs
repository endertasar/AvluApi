using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("admin/register")]
    [AllowAnonymous]
    public async Task<ActionResult<AdminRegisterResponse>> AdminRegister([FromBody] AdminRegisterRequest req)
    {
        var result = await _auth.RegisterAdminAsync(req);
        return Ok(result);
    }

    [HttpPost("admin/login")]
    [AllowAnonymous]
    public async Task<ActionResult<AdminLoginResponse>> AdminLogin([FromBody] AdminLoginRequest req)
    {
        var result = await _auth.LoginAdminAsync(req);
        return Ok(result);
    }

    [HttpPost("resident/register")]
    [AllowAnonymous]
    public async Task<ActionResult<ResidentLoginResponse>> ResidentRegister([FromBody] ResidentRegisterRequest req)
    {
        var result = await _auth.RegisterResidentAsync(req);
        return Ok(result);
    }

    [HttpPost("resident/login")]
    [AllowAnonymous]
    public async Task<ActionResult<ResidentLoginResponse>> ResidentLogin([FromBody] ResidentLoginRequest req)
    {
        var result = await _auth.LoginResidentAsync(req);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenRefreshResponse>> Refresh([FromBody] RefreshTokenRequest req)
    {
        var result = await _auth.RefreshTokenAsync(req.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
    {
        await _auth.LogoutAsync(req.RefreshToken);
        return NoContent();
    }
}
