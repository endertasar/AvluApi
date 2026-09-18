using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IResidentRepository
{
    Task<Resident?> GetByIdAsync(long id);
    Task<Resident?> GetByPhoneAsync(string phone);
    Task<long> CreateAsync(Resident resident);
    Task UpdateAsync(Resident resident);
    Task SoftDeleteAsync(long id);
}
