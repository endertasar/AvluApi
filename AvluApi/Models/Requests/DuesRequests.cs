namespace AvluApi.Models.Requests;

public class CreateDuesDefinitionRequest
{
    public string PropertyType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int EffectiveFrom { get; set; }
    public string? Description { get; set; }
}

public class CreateChargeRequest
{
    public long PropertyId { get; set; }
    public int Period { get; set; }
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
}

public class GenerateChargesRequest
{
    public int Period { get; set; }
}
