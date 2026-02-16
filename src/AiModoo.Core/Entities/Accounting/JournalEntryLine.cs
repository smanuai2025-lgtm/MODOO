using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class JournalEntryLine : BaseEntity
{
    public int JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int? PartnerId { get; set; }
    public PartnerType PartnerType { get; set; } = PartnerType.None;
    public string? Label { get; set; }
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    public decimal? AmountCurrency { get; set; }
    public bool IsReconciled { get; set; }
    public int? ReconcileGroupId { get; set; }
}
