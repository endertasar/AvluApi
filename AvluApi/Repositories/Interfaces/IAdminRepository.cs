using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IAdminRepository
{
    Task<AdminUser?> GetByIdAsync(long id);
    Task<IEnumerable<AdminUser>> GetBySiteAsync();
    Task<long> CreateAsync(AdminUser user);
    Task UpdateAsync(long id, string? fullName, string role, bool isActive);
    Task SoftDeleteAsync(long id);
}
