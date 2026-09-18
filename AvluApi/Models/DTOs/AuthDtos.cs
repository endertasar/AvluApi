namespace AvluApi.Models.DTOs;

public record AdminRegisterResponse(
    SiteDto Site,
    string AccessToken,
    string RefreshToken);

public record AdminLoginResponse(
    string AccessToken,
    string RefreshToken,
    AdminUserInfo User);

public record ResidentLoginResponse(
    string AccessToken,
    string RefreshToken,
    ResidentInfo Resident);

public record TokenRefreshResponse(
    string AccessToken,
    string RefreshToken);

public record AdminUserInfo(
    long Id,
    string Username,
    string? FullName,
    string Role,
    bool IsActive);

public record ResidentInfo(
    long Id,
    string Phone,
    string? FullName);

public record SiteDto(
    long Id,
    string SiteUsername,
    string Name);
