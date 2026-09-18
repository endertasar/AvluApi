namespace AvluApi.Models.Requests;

public class CreateExpenseRequest
{
    public string?  Category    { get; set; }
    public decimal  Amount      { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string?  Description { get; set; }
}

public class UpdateExpenseRequest
{
    public string?  Category    { get; set; }
    public decimal  Amount      { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string?  Description { get; set; }
}
