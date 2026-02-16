using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Purchases;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class Invoice : AuditableEntity
{
    public string Number { get; set; } = string.Empty;
    public InvoiceType InvoiceType { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int? PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public PartnerType PartnerType { get; set; }
    public string? PartnerName { get; set; }
    public string? PartnerNameAr { get; set; }

    // Sales Order link
    public int? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    // Purchase Order link
    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int JournalId { get; set; }
    public Journal Journal { get; set; } = null!;
    public int? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public int? FiscalPeriodId { get; set; }
    public FiscalPeriod? FiscalPeriod { get; set; }
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }

    // Status
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // References
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? NotesAr { get; set; }
    public string? SourceDocument { get; set; }
    public string? SourceModule { get; set; }

    // Reversal
    public int? ReversalOfId { get; set; }
    public Invoice? ReversalOf { get; set; }

    // Collections
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
}
