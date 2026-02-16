using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class BankReconciliation : AuditableEntity
{
    public int BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal StartingBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.InProgress;
    public ICollection<BankReconciliationLine> Lines { get; set; } = new List<BankReconciliationLine>();
}
