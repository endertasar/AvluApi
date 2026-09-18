using AvluApi.Models.Entities;

namespace AvluApi.Repositories.Interfaces;

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetAllAsync(DateTime? dateFrom, DateTime? dateTo, string? category, CancellationToken ct = default);
    Task<Expense?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<long> CreateAsync(Expense expense, CancellationToken ct = default);
    Task UpdateAsync(Expense expense, CancellationToken ct = default);
    Task SoftDeleteAsync(long id, CancellationToken ct = default);
}
