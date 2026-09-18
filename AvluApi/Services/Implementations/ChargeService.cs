using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class ChargeService : IChargeService
{
    private readonly IChargeRepository _chargeRepo;

    public ChargeService(IChargeRepository chargeRepo)
    {
        _chargeRepo = chargeRepo;
    }

    public async Task<IEnumerable<DuesChargeDto>> GetAllAsync(long? propertyId, string? status, int? period)
    {
        var charges = await _chargeRepo.GetAllAsync(propertyId, status, period);
        return charges.Select(MapDto);
    }

    public async Task<DuesChargeDto> GetByIdAsync(long id)
    {
        var charge = await _chargeRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Borç kaydı bulunamadı.");
        return MapDto(charge);
    }

    public Task<ChargeSummaryDto> GetSummaryAsync() => _chargeRepo.GetSummaryAsync();

    private static DuesChargeDto MapDto(DuesCharge c) => new()
    {
        Id = c.Id,
        PropertyId = c.PropertyId,
        Period = c.Period,
        Amount = c.Amount,
        PaidAmount = c.PaidAmount,
        DueDate = c.DueDate,
        Status = c.Status,
        CreatedAt = c.CreatedAt
    };
}
