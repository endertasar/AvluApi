namespace AvluApi.Models.Entities;

public class Expense
{
    public long      Id              { get; set; }
    public long      SiteId          { get; set; }
    public string?   Category        { get; set; }
    public decimal   Amount          { get; set; }
    public DateTime  ExpenseDate     { get; set; }
    public string?   Description     { get; set; }
    public long?     CreatedByUserId { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public bool      IsDeleted       { get; set; }
}
