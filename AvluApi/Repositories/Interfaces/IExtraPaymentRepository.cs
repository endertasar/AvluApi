using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IExtraPaymentRepository
{
    Task<IEnumerable<ExtraPayment>> GetAllAsync(CancellationToken ct = default);
    Task<ExtraPayment?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<long> CreateAsync(ExtraPayment ep, CancellationToken ct = default);
    Task<IEnumerable<ExtraPaymentCharge>> GetChargesAsync(long extraPaymentId, long? propertyId, string? status, CancellationToken ct = default);
    Task<ExtraPaymentCharge?> GetChargeByIdAsync(long id, CancellationToken ct = default);
    Task<int> BulkInsertChargesAsync(IEnumerable<ExtraPaymentCharge> charges, CancellationToken ct = default);
    Task UpdateChargeCountAsync(long id, int count, decimal totalAmount, CancellationToken ct = default);
}
