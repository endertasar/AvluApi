namespace AvluApi.Models.Entities;

public class PropertyBalance
{
    public long    Id            { get; set; }
    public long    SiteId        { get; set; }
    public long    PropertyId    { get; set; }
    public decimal CreditBalance { get; set; }
    public DateTime UpdatedAt   { get; set; }
}
