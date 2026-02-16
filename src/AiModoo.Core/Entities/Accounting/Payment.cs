using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class Payment : AuditableEntity
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public int PaymentMethodId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;
    public int? PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public PartnerType PartnerType { get; set; }
    public string? PartnerName { get; set; }
    public int JournalId { get; set; }
    public Journal Journal { get; set; } = null!;
    public int? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Draft;
    public string? Memo { get; set; }
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    // Allocation tracking
    public decimal AllocatedAmount { get; set; }
    public decimal UnallocatedAmount => Amount - AllocatedAmount;
    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
