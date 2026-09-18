namespace AvluApi.Models.DTOs;

public class DuesChargeDto
{
    public long Id { get; set; }
    public long PropertyId { get; set; }
    public int Period { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ChargeSummaryDto
{
    public decimal TotalDebt { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOverdue { get; set; }
    public int OverdueCount { get; set; }
}
