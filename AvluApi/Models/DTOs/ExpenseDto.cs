namespace AvluApi.Models.DTOs;

public class ExpenseDto
{
    public long     Id              { get; set; }
    public string?  Category        { get; set; }
    public decimal  Amount          { get; set; }
    public DateTime ExpenseDate     { get; set; }
    public string?  Description     { get; set; }
    public long?    CreatedByUserId { get; set; }
    public DateTime CreatedAt       { get; set; }
}
