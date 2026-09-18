using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IPropertyRepository
{
    Task<IEnumerable<Property>> GetAllAsync();
    Task<Property?> GetByIdAsync(long id);
    Task<long> CreateAsync(Property property);
    Task UpdateAsync(Property property);
    Task SoftDeleteAsync(long id);
}
