using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Inventory;

public class InventoryAdjustmentLine : BaseEntity
{
    public int AdjustmentId { get; set; }
    public InventoryAdjustment Adjustment { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public int? LotId { get; set; }
    public StockLot? Lot { get; set; }

    public decimal TheoreticalQty { get; set; }
    public decimal RealQty { get; set; }
    public decimal Difference => RealQty - TheoreticalQty;
}
