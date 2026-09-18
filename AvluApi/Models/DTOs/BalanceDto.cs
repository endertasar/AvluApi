namespace AvluApi.Models.DTOs;

public record CreatePaymentResponse(
    long     Id,
    long     PropertyId,
    string   TargetType,
    long     TargetId,
    decimal  Amount,
    string   Method,
    DateTime PaidAt,
    string   ChargeStatus,
    decimal  ChargeRemaining,
    decimal  AppliedToCredit,
    decimal  CreditBalance);

public class PropertyBalanceDto
{
    public long                         PropertyId    { get; init; }
    public decimal                      CreditBalance { get; init; }
    public IEnumerable<BalanceTxDto>    Transactions  { get; init; } = [];
}

public record BalanceTxDto(
    string   Direction,
    decimal  Amount,
    string   Source,
    string?  Note,
    DateTime CreatedAt);
