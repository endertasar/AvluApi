namespace AvluApi.Models.DTOs;

public class ExtraPaymentDto
{
    public long     Id                   { get; set; }
    public string   Name                 { get; set; } = string.Empty;
    public decimal  AmountPerProperty    { get; set; }
    public decimal? TotalAmount          { get; set; }
    public int      InstallmentCount     { get; set; }
    public int      GeneratedChargeCount { get; set; }
    public DateTime CreatedAt            { get; set; }
}

public class ExtraPaymentDetailDto
{
    public long     Id                { get; set; }
    public string   Name              { get; set; } = string.Empty;
    public decimal  AmountPerProperty { get; set; }
    public decimal? TotalAmount       { get; set; }
    public int      InstallmentCount  { get; set; }
    public DateTime CreatedAt         { get; set; }

    public IEnumerable<InstallmentSummaryDto> Installments { get; set; } = [];
}

public class InstallmentSummaryDto
{
    public int     InstallmentNo  { get; set; }
    public decimal Amount         { get; set; }
    public decimal PaidAmount     { get; set; }
    public int     TotalCharges   { get; set; }
    public int     PaidCharges    { get; set; }
}

public class ExtraPaymentChargeDto
{
    public long      Id             { get; set; }
    public long      ExtraPaymentId { get; set; }
    public long      PropertyId     { get; set; }
    public int       InstallmentNo  { get; set; }
    public decimal   Amount         { get; set; }
    public decimal   PaidAmount     { get; set; }
    public string    Status         { get; set; } = string.Empty;
    public DateTime? DueDate        { get; set; }
    public DateTime  CreatedAt      { get; set; }
}
