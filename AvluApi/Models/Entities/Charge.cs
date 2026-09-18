namespace AvluApi.Models.Entities;

public class DuesCharge
{
    public long Id { get; set; }
    public long SiteId { get; set; }
    public long PropertyId { get; set; }
    public int Period { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
