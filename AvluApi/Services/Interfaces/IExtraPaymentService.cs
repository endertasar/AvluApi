using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IExtraPaymentService
{
    Task<IEnumerable<ExtraPaymentDto>>   GetAllAsync(CancellationToken ct = default);
    Task<ExtraPaymentDetailDto>          GetByIdAsync(long id, CancellationToken ct = default);
    Task<ExtraPaymentDto>                CreateAsync(CreateExtraPaymentRequest request, CancellationToken ct = default);
    Task<IEnumerable<ExtraPaymentChargeDto>> GetChargesAsync(long id, long? propertyId, string? status, CancellationToken ct = default);
}
