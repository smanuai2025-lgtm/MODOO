using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class BankStatementLine : BaseEntity
{
    public int StatementId { get; set; }
    public BankStatement Statement { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public string? PartnerName { get; set; }
    public decimal Amount { get; set; }
    public bool IsReconciled { get; set; }
    public int? MatchedJournalEntryLineId { get; set; }
    public JournalEntryLine? MatchedJournalEntryLine { get; set; }
    public int? ReconciliationId { get; set; }
    public BankReconciliation? Reconciliation { get; set; }
}
