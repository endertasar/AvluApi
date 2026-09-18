using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync(long? propertyId, DateTime? dateFrom, DateTime? dateTo);
    Task<Payment?> GetByIdAsync(long id);
    Task<IEnumerable<Payment>> GetByTargetAsync(string targetType, long targetId);
    Task<long> CreateAsync(Payment payment);
    Task SoftDeleteAsync(long id);
}
