using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Inventory;

namespace AiModoo.Core.Entities.POS;

public class PosOrderLine : BaseEntity
{
    public int OrderId { get; set; }
    public PosOrder Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;

    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
