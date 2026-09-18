using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IPropertyService
{
    Task<IEnumerable<PropertyDto>> GetAllAsync(CancellationToken ct = default);
    Task<PropertyDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<PropertyDto> CreateAsync(CreatePropertyRequest request, CancellationToken ct = default);
    Task<PropertyDto> UpdateAsync(long id, UpdatePropertyRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<PropertyBalanceDto> GetBalanceAsync(long id, CancellationToken ct = default);
}
