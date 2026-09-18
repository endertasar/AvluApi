using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IChargeRepository
{
    Task<IEnumerable<DuesCharge>> GetAllAsync(long? propertyId, string? status, int? period);
    Task<DuesCharge?> GetByIdAsync(long id);
    Task<ChargeSummaryDto> GetSummaryAsync();
    Task<long> CreateAsync(DuesCharge charge);
    Task UpdatePaidAmountAsync(long id, decimal paidAmount, string status);
}
