using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllAsync(long? propertyId, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default);
    Task<PaymentDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CreatePaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default);
    Task CancelAsync(long id, CancellationToken ct = default);
}
