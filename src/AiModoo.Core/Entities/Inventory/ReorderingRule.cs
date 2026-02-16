using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Inventory;

public class ReorderingRule : AuditableEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public decimal MinQty { get; set; }
    public decimal MaxQty { get; set; }
    public decimal OrderQty { get; set; }
    public int LeadDays { get; set; }
    public bool IsActive { get; set; } = true;
}
