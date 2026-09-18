using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IResidentService
{
    Task<ResidentDto> GetByIdAsync(long id);
    Task<ResidentDto> GetByPhoneAsync(string phone);
    Task<ResidentDto> CreateAsync(CreateResidentRequest request);
    Task UpdateAsync(long id, UpdateResidentRequest request);
    Task DeleteAsync(long id);
}
