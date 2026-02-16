using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Inventory;

namespace AiModoo.Core.Entities.Sales;

public class SalesOrderLine : BaseEntity
{
    public int OrderId { get; set; }
    public SalesOrder Order { get; set; } = null!;
    public int Sequence { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }

    public decimal Quantity { get; set; } = 1;
    public decimal DeliveredQty { get; set; }
    public decimal InvoicedQty { get; set; }

    public int? UomId { get; set; }
    public UnitOfMeasure? Uom { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }

    public int? TaxId { get; set; }
    public Tax? Tax { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
