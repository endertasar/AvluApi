namespace AvluApi.Models.Entities;

public class ExtraPayment
{
    public long     Id                { get; set; }
    public long     SiteId            { get; set; }
    public string   Name              { get; set; } = string.Empty;
    public decimal  AmountPerProperty { get; set; }
    public decimal? TotalAmount       { get; set; }
    public int      InstallmentCount  { get; set; } = 1;
    public DateTime CreatedAt         { get; set; }
    public bool     IsDeleted         { get; set; }
}
