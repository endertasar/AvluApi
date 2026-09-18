using AvluApi.Models.DTOs;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvluApi.Controllers;

[ApiController]
[Route("api/v1/expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service)
    {
        _service = service;
    }

    /// <summary>Gider listesi — Manager/SuperAdmin (filtre: dateFrom, dateTo, category)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExpenseDto>>>> GetAll(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string?   category,
        CancellationToken ct)
    {
        var expenses = await _service.GetAllAsync(dateFrom, dateTo, category, ct);
        return Ok(ApiResponse<IEnumerable<ExpenseDto>>.Ok(expenses));
    }

    /// <summary>Gider detayı — Manager/SuperAdmin</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> GetById(long id, CancellationToken ct)
    {
        var expense = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ExpenseDto>.Ok(expense));
    }

    /// <summary>Gider kaydet — Manager/SuperAdmin</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Create(
        [FromBody] CreateExpenseRequest request, CancellationToken ct)
    {
        var expense = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id },
            ApiResponse<ExpenseDto>.Ok(expense, "Gider kaydedildi."));
    }

    /// <summary>Gider güncelle — Manager/SuperAdmin</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Update(
        long id, [FromBody] UpdateExpenseRequest request, CancellationToken ct)
    {
        var expense = await _service.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ExpenseDto>.Ok(expense, "Gider güncellendi."));
    }

    /// <summary>Gider sil (soft delete) — Manager/SuperAdmin</summary>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.Ok("Gider silindi."));
    }
}
