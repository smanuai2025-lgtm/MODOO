namespace AiModoo.Core.DTOs.Accounting;

public class TrialBalanceDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNameAr { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}

public class TrialBalanceReportDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<TrialBalanceDto> Lines { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}
