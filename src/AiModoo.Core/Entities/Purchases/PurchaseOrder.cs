using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Purchases;

public class PurchaseOrder : AuditableEntity
{
    public string Number { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.RFQ;

    // Vendor (Partner)
    public int VendorId { get; set; }
    public Partner Vendor { get; set; } = null!;

    // Dates
    public DateTime OrderDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? ReceiptDate { get; set; }

    // Payment
    public int? PaymentTermId { get; set; }
    public PaymentTerm? PaymentTerm { get; set; }
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    // Totals
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }

    // References
    public string? VendorReference { get; set; }
    public string? Notes { get; set; }
    public string? NotesAr { get; set; }

    // Linked documents
    public int? BillId { get; set; }
    public Invoice? Bill { get; set; }
    public int? ReceiptPickingId { get; set; }
    public StockPicking? ReceiptPicking { get; set; }

    // Lines
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
