namespace AvluApi.Models.Requests;

public class CreatePaymentRequest
{
    public long     PropertyId  { get; set; }
    public string   TargetType  { get; set; } = string.Empty; // Dues | Extra
    public long     TargetId    { get; set; }
    public decimal  Amount      { get; set; }
    public string?  Method      { get; set; }
    public string?  Note        { get; set; }
    public bool?    UseCredit   { get; set; }
}
