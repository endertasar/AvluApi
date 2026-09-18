namespace AvluApi.Models.DTOs;

public class MonthlyCollectionDto
{
    public int     Year            { get; set; }
    public int     Month           { get; set; }
    public decimal TotalCharged    { get; set; }
    public decimal TotalCollected  { get; set; }
    public decimal TotalPending    { get; set; }
    public int     TotalProperties { get; set; }
}

public class DebtAgingDto
{
    public decimal Days0To30  { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal DaysOver90 { get; set; }
}

public class PropertySummaryDto
{
    public long     PropertyId { get; set; }
    public string?  Block      { get; set; }
    public string   UnitNo     { get; set; } = string.Empty;
    public decimal  TotalDebt  { get; set; }
    public decimal  TotalPaid  { get; set; }
    public decimal  Balance    => TotalDebt - TotalPaid;
}

public class OverdueChargeDto
{
    public long      ChargeId    { get; set; }
    public long      PropertyId  { get; set; }
    public string?   Block       { get; set; }
    public string    UnitNo      { get; set; } = string.Empty;
    public decimal   Amount      { get; set; }
    public decimal   PaidAmount  { get; set; }
    public DateTime? DueDate     { get; set; }
    public int       DaysOverdue { get; set; }
    public string    Status      { get; set; } = string.Empty;
}

public class DashboardDto
{
    public int     TotalActiveProperties    { get; set; }
    public decimal TotalPendingDues         { get; set; }
    public decimal CurrentMonthCharged      { get; set; }
    public decimal CurrentMonthCollected    { get; set; }
    public int     OverdueCount             { get; set; }
    public decimal TotalExpensesYtd         { get; set; }
    public decimal TotalCreditBalance       { get; set; }
}

public class IncomeExpenseMonthDto
{
    public int     Year        { get; set; }
    public int     Month       { get; set; }
    public decimal IncomeDues  { get; set; }
    public decimal IncomeExtra { get; set; }
    public decimal Expense     { get; set; }
    public decimal Net         => IncomeDues + IncomeExtra - Expense;
}
