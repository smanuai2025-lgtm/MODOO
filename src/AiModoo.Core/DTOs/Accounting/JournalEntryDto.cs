using AiModoo.Core.Enums;

namespace AiModoo.Core.DTOs.Accounting;

public class JournalEntryDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string JournalName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public string? Narration { get; set; }
    public JournalEntryStatus Status { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

public class JournalEntryLineDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Label { get; set; }
    public string? PartnerName { get; set; }
}
