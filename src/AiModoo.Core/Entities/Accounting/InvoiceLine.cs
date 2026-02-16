using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Inventory;

namespace AiModoo.Core.Entities.Accounting;

public class InvoiceLine : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int Sequence { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
    public int? TaxId { get; set; }
    public Tax? Tax { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    // Product reference (integration with Inventory module)
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public string? ProductName { get; set; }
}
