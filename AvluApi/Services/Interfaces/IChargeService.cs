using AvluApi.Models.DTOs;

namespace AvluApi.Services.Interfaces;

public interface IChargeService
{
    Task<IEnumerable<DuesChargeDto>> GetAllAsync(long? propertyId, string? status, int? period);
    Task<DuesChargeDto> GetByIdAsync(long id);
    Task<ChargeSummaryDto> GetSummaryAsync();
}
