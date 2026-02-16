using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Inventory;

public class StockQuant : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public int? LotId { get; set; }
    public StockLot? Lot { get; set; }

    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => Quantity - ReservedQuantity;

    public DateTime? InDate { get; set; }
}
