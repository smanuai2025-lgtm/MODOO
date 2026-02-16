using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class JournalEntry : AuditableEntity
{
    public int JournalId { get; set; }
    public Journal Journal { get; set; } = null!;
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public string? Narration { get; set; }
    public string? NarrationAr { get; set; }
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    public int? FiscalPeriodId { get; set; }
    public FiscalPeriod? FiscalPeriod { get; set; }
    public string? SourceDocument { get; set; }
    public string? SourceModule { get; set; }
    public string? PostedById { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? ReversalOfId { get; set; }
    public JournalEntry? ReversalOf { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
