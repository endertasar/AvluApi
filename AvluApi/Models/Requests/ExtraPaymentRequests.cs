namespace AvluApi.Models.Requests;

public class CreateExtraPaymentRequest
{
    public string        Name              { get; set; } = string.Empty;
    public decimal       AmountPerProperty { get; set; }
    public int           InstallmentCount  { get; set; } = 1;
    public string        PropertyScope     { get; set; } = "All"; // All | Selected
    public List<long>?   PropertyIds       { get; set; }
}
