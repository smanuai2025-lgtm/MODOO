using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Sales;

public class SalesOrder : AuditableEntity
{
    public string Number { get; set; } = string.Empty;
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Quotation;

    // Partner
    public int PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;

    // Dates
    public DateTime OrderDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? ExpirationDate { get; set; }

    // Pricing
    public int? PricelistId { get; set; }
    public Pricelist? Pricelist { get; set; }
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    // Payment
    public int? PaymentTermId { get; set; }
    public PaymentTerm? PaymentTerm { get; set; }
    public InvoicingPolicy InvoicingPolicy { get; set; } = InvoicingPolicy.OrderedQuantities;

    // Totals
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }

    // References
    public string? ClientOrderRef { get; set; }
    public string? Notes { get; set; }
    public string? NotesAr { get; set; }

    // Linked documents
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public int? DeliveryPickingId { get; set; }
    public StockPicking? DeliveryPicking { get; set; }

    // Lines
    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
