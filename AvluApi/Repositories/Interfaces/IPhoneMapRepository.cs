using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IPhoneMapRepository
{
    Task<IEnumerable<Property>> GetPropertiesByPhoneAsync(string phone);
}
