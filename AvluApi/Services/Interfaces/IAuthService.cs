using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IAuthService
{
    Task<AdminRegisterResponse> RegisterAdminAsync(AdminRegisterRequest request);
    Task<AdminLoginResponse> LoginAdminAsync(AdminLoginRequest request);
    Task<ResidentLoginResponse> RegisterResidentAsync(ResidentRegisterRequest request);
    Task<ResidentLoginResponse> LoginResidentAsync(ResidentLoginRequest request);
    Task<TokenRefreshResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}
