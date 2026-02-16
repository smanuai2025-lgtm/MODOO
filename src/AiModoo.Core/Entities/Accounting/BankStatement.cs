using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class BankStatement : AuditableEntity
{
    public int BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public string? Reference { get; set; }
    public DateTime Date { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public bool IsProcessed { get; set; }
    public ICollection<BankStatementLine> Lines { get; set; } = new List<BankStatementLine>();
}
