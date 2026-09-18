using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;

namespace AvluApi.Services.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllAsync(DateTime? dateFrom, DateTime? dateTo, string? category, CancellationToken ct = default);
    Task<ExpenseDto>              GetByIdAsync(long id, CancellationToken ct = default);
    Task<ExpenseDto>              CreateAsync(CreateExpenseRequest request, CancellationToken ct = default);
    Task<ExpenseDto>              UpdateAsync(long id, UpdateExpenseRequest request, CancellationToken ct = default);
    Task                          DeleteAsync(long id, CancellationToken ct = default);
}
