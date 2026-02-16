using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Inventory;

public class StockMove : AuditableEntity
{
    public string? Reference { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal ProductUomQty { get; set; }
    public int? UomId { get; set; }
    public UnitOfMeasure? Uom { get; set; }

    public int? PickingId { get; set; }
    public StockPicking? Picking { get; set; }

    public int SourceLocationId { get; set; }
    public Location SourceLocation { get; set; } = null!;

    public int DestLocationId { get; set; }
    public Location DestLocation { get; set; } = null!;

    public StockMoveStatus Status { get; set; } = StockMoveStatus.Draft;
    public DateTime Date { get; set; }

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public int? LotId { get; set; }
    public StockLot? Lot { get; set; }
}
