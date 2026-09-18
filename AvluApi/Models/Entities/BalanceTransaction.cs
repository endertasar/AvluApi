namespace AvluApi.Models.Entities;

public class BalanceTransaction
{
    public long    Id         { get; set; }
    public long    SiteId     { get; set; }
    public long    PropertyId { get; set; }
    public string  Direction  { get; set; } = string.Empty; // Credit | Debit
    public decimal Amount     { get; set; }
    public string  Source     { get; set; } = string.Empty; // Overpayment | AppliedToCharge | Manual
    public long?   SourceId   { get; set; }
    public string? Note       { get; set; }
    public DateTime CreatedAt { get; set; }
}
