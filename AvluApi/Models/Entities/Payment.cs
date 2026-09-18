namespace AvluApi.Models.Entities;

public class Payment
{
    public long Id { get; set; }
    public long SiteId { get; set; }
    public long PropertyId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public long TargetId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public string? Note { get; set; }
    public DateTime PaidAt { get; set; }
    public long? CollectedByUserId { get; set; }
    public bool IsDeleted { get; set; }
}
