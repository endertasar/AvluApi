using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IPropertyBalanceRepository
{
    Task<decimal> GetCreditBalanceAsync(long propertyId, CancellationToken ct = default);
    Task<IEnumerable<BalanceTransaction>> GetTransactionsAsync(long propertyId, CancellationToken ct = default);
}
