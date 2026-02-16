using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Inventory;

public class InventoryAdjustment : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public InventoryAdjustmentStatus Status { get; set; } = InventoryAdjustmentStatus.Draft;

    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public DateTime Date { get; set; }
    public DateTime? AccountingDate { get; set; }
    public string? Notes { get; set; }

    public ICollection<InventoryAdjustmentLine> Lines { get; set; } = new List<InventoryAdjustmentLine>();
}
