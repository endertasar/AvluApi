using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class ResidentService : IResidentService
{
    private readonly IResidentRepository _residentRepo;

    public ResidentService(IResidentRepository residentRepo)
    {
        _residentRepo = residentRepo;
    }

    public async Task<ResidentDto> GetByIdAsync(long id)
    {
        var resident = await _residentRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Sakin bulunamadı.");
        return MapDto(resident);
    }

    public async Task<ResidentDto> GetByPhoneAsync(string phone)
    {
        var resident = await _residentRepo.GetByPhoneAsync(phone)
            ?? throw new NotFoundException("Sakin bulunamadı.");
        return MapDto(resident);
    }

    public async Task<ResidentDto> CreateAsync(CreateResidentRequest request)
    {
        var resident = new Resident
        {
            Phone = request.Phone,
            PasswordHash = request.PasswordHash,
            FullName = request.FullName
        };
        resident.Id = await _residentRepo.CreateAsync(resident);
        return MapDto(resident);
    }

    public async Task UpdateAsync(long id, UpdateResidentRequest request)
    {
        var resident = await _residentRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Sakin bulunamadı.");

        resident.FullName = request.FullName;
        resident.OneSignalUserId = request.OneSignalUserId;
        await _residentRepo.UpdateAsync(resident);
    }

    public async Task DeleteAsync(long id)
    {
        _ = await _residentRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Sakin bulunamadı.");
        await _residentRepo.SoftDeleteAsync(id);
    }

    private static ResidentDto MapDto(Resident r) => new()
    {
        Id = r.Id,
        Phone = r.Phone,
        FullName = r.FullName,
        IsPhoneVerified = r.IsPhoneVerified,
        CreatedAt = r.CreatedAt
    };
}
