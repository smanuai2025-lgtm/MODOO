namespace AiModoo.Core.DTOs.Accounting;

public class FinancialReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<FinancialReportSection> Sections { get; set; } = new();
    public decimal GrandTotal { get; set; }
}

public class FinancialReportSection
{
    public string Title { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public List<FinancialReportLine> Lines { get; set; } = new();
    public decimal SectionTotal { get; set; }
}

public class FinancialReportLine
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNameAr { get; set; }
    public decimal Amount { get; set; }
    public int Level { get; set; }
}

public class GeneralLedgerDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? JournalEntryNumber { get; set; }
    public string? Reference { get; set; }
    public string? Label { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

public class PartnerStatementDto
{
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string? PartnerNameAr { get; set; }
    public string PartnerType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<PartnerStatementLineDto> Lines { get; set; } = new();
}

public class PartnerStatementLineDto
{
    public DateTime Date { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

public class AccountStatementDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNameAr { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<AccountStatementLineDto> Lines { get; set; } = new();
}

public class AccountStatementLineDto
{
    public DateTime Date { get; set; }
    public string JournalEntryNumber { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Label { get; set; }
    public string? PartnerName { get; set; }
    public string? SourceModule { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

public class AgedBalanceDto
{
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90 { get; set; }
    public decimal Total { get; set; }
}
