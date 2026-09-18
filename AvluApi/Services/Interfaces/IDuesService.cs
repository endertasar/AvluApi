using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IDuesService
{
    Task<IEnumerable<DuesDefinition>> GetDefinitionsAsync();
    Task<DuesDefinition> CreateDefinitionAsync(CreateDuesDefinitionRequest request);
    Task SoftDeleteDefinitionAsync(long id);
    Task<DuesChargeDto> CreateChargeAsync(CreateChargeRequest request);
    Task<GenerateChargesResultDto> GenerateChargesAsync(int period, CancellationToken ct = default);
}
