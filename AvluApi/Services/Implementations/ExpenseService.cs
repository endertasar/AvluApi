using System.Text.Json;
using AvluApi.Auth;
using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository  _repo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly ISiteContext        _siteContext;

    public ExpenseService(IExpenseRepository repo, IAuditLogRepository auditRepo, ISiteContext siteContext)
    {
        _repo        = repo;
        _auditRepo   = auditRepo;
        _siteContext = siteContext;
    }

    public async Task<IEnumerable<ExpenseDto>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, string? category, CancellationToken ct = default)
    {
        var expenses = await _repo.GetAllAsync(dateFrom, dateTo, category, ct);
        return expenses.Select(MapDto);
    }

    public async Task<ExpenseDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var expense = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Gider bulunamadı.");
        return MapDto(expense);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new DomainException("Tutar sıfırdan büyük olmalıdır.");

        var expense = new Expense
        {
            Category        = request.Category,
            Amount          = request.Amount,
            ExpenseDate     = request.ExpenseDate,
            Description     = request.Description,
            CreatedByUserId = _siteContext.UserId > 0 ? (long?)_siteContext.UserId : null
        };

        expense.Id = await _repo.CreateAsync(expense, ct);

        await _auditRepo.WriteAsync(
            action:   "Create",
            entity:   "Expense",
            siteId:   _siteContext.SiteId,
            entityId: expense.Id,
            userType: _siteContext.UserType,
            userId:   _siteContext.UserId > 0 ? (long?)_siteContext.UserId : null,
            detail:   JsonSerializer.Serialize(new { request.Category, request.Amount, request.ExpenseDate }),
            ct:       ct);

        return MapDto(expense);
    }

    public async Task<ExpenseDto> UpdateAsync(long id, UpdateExpenseRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new DomainException("Tutar sıfırdan büyük olmalıdır.");

        var expense = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Gider bulunamadı.");

        expense.Category    = request.Category;
        expense.Amount      = request.Amount;
        expense.ExpenseDate = request.ExpenseDate;
        expense.Description = request.Description;

        await _repo.UpdateAsync(expense, ct);

        await _auditRepo.WriteAsync(
            action:   "Update",
            entity:   "Expense",
            siteId:   _siteContext.SiteId,
            entityId: expense.Id,
            userType: _siteContext.UserType,
            userId:   _siteContext.UserId > 0 ? (long?)_siteContext.UserId : null,
            detail:   JsonSerializer.Serialize(new { request.Category, request.Amount, request.ExpenseDate }),
            ct:       ct);

        return MapDto(expense);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        _ = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Gider bulunamadı.");

        await _repo.SoftDeleteAsync(id, ct);

        await _auditRepo.WriteAsync(
            action:   "Delete",
            entity:   "Expense",
            siteId:   _siteContext.SiteId,
            entityId: id,
            userType: _siteContext.UserType,
            userId:   _siteContext.UserId > 0 ? (long?)_siteContext.UserId : null,
            ct:       ct);
    }

    private static ExpenseDto MapDto(Expense e) => new()
    {
        Id              = e.Id,
        Category        = e.Category,
        Amount          = e.Amount,
        ExpenseDate     = e.ExpenseDate,
        Description     = e.Description,
        CreatedByUserId = e.CreatedByUserId,
        CreatedAt       = e.CreatedAt
    };
}
