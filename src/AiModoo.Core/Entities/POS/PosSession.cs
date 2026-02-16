using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.POS;

public class PosSession : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public PosSessionStatus Status { get; set; } = PosSessionStatus.Opening;

    public int ConfigId { get; set; }
    public PosConfig Config { get; set; } = null!;

    public string? UserId { get; set; }
    public string? UserName { get; set; }

    public DateTime OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public int OrderCount { get; set; }

    public int? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    public string? Notes { get; set; }

    public ICollection<PosOrder> Orders { get; set; } = new List<PosOrder>();
}
