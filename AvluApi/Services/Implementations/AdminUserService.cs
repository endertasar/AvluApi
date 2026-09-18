using BCrypt.Net;
using AvluApi.Auth;
using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class AdminUserService : IAdminUserService
{
    private readonly IAdminRepository _adminRepo;
    private readonly ISiteContext _siteContext;

    public AdminUserService(IAdminRepository adminRepo, ISiteContext siteContext)
    {
        _adminRepo   = adminRepo;
        _siteContext = siteContext;
    }

    public async Task<IEnumerable<AdminUserInfo>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _adminRepo.GetBySiteAsync();
        return users.Select(MapDto);
    }

    public async Task<AdminUserInfo> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var user = await GetOwnedUserAsync(id);
        return MapDto(user);
    }

    public async Task<AdminUserInfo> CreateAsync(CreateAdminUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new DomainException("Kullanıcı adı zorunludur.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new DomainException("Şifre en az 6 karakter olmalıdır.");
        if (request.Role != "Owner" && request.Role != "SubUser")
            throw new DomainException("Rol Owner veya SubUser olmalıdır.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new AdminUser
        {
            SiteId       = _siteContext.SiteId,
            Username     = request.Username.Trim(),
            PasswordHash = passwordHash,
            FullName     = request.FullName?.Trim(),
            Role         = request.Role
        };
        user.Id = await _adminRepo.CreateAsync(user);
        return MapDto(user);
    }

    public async Task<AdminUserInfo> UpdateAsync(long id, UpdateAdminUserRequest request, CancellationToken ct = default)
    {
        if (request.Role != "Owner" && request.Role != "SubUser")
            throw new DomainException("Rol Owner veya SubUser olmalıdır.");

        var user = await GetOwnedUserAsync(id);
        await _adminRepo.UpdateAsync(id, request.FullName?.Trim(), request.Role, request.IsActive);
        user.FullName = request.FullName?.Trim();
        user.Role     = request.Role;
        user.IsActive = request.IsActive;
        return MapDto(user);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var user = await GetOwnedUserAsync(id);
        if (user.Id == _siteContext.UserId)
            throw new DomainException("Kendi hesabınızı silemezsiniz.");
        await _adminRepo.SoftDeleteAsync(id);
    }

    private async Task<AdminUser> GetOwnedUserAsync(long id)
    {
        var user = await _adminRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");
        if (user.SiteId != _siteContext.SiteId)
            throw new NotFoundException("Kullanıcı bulunamadı.");
        return user;
    }

    private static AdminUserInfo MapDto(AdminUser u) =>
        new(u.Id, u.Username, u.FullName, u.Role, u.IsActive);
}
