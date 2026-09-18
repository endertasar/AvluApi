using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IAdminUserService
{
    Task<IEnumerable<AdminUserInfo>> GetAllAsync(CancellationToken ct = default);
    Task<AdminUserInfo> GetByIdAsync(long id, CancellationToken ct = default);
    Task<AdminUserInfo> CreateAsync(CreateAdminUserRequest request, CancellationToken ct = default);
    Task<AdminUserInfo> UpdateAsync(long id, UpdateAdminUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
