using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

/// <summary>
/// Links a payment to one or more invoices (supports partial payment)
/// </summary>
public class PaymentAllocation : BaseEntity
{
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime AllocationDate { get; set; }
}
