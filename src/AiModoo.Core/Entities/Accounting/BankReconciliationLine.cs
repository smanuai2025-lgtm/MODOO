using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class BankReconciliationLine : BaseEntity
{
    public int ReconciliationId { get; set; }
    public BankReconciliation Reconciliation { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int? JournalEntryLineId { get; set; }
    public JournalEntryLine? JournalEntryLine { get; set; }
    public bool IsMatched { get; set; }
    public string? PartnerName { get; set; }
}
