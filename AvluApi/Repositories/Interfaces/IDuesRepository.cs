using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IDuesRepository
{
    Task<IEnumerable<DuesDefinition>> GetAllAsync();
    Task<DuesDefinition?> GetByIdAsync(long id);
    Task<DuesDefinition?> GetActiveForTypeAndPeriodAsync(long siteId, string propertyType, int period);
    Task<long> CreateAsync(DuesDefinition def);
    Task SoftDeleteAsync(long id);
}
