using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Inventory;

public class StockPickingType : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public PickingTypeCode Code { get; set; }
    public string Sequence { get; set; } = string.Empty;

    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int? DefaultSourceLocationId { get; set; }
    public Location? DefaultSourceLocation { get; set; }

    public int? DefaultDestLocationId { get; set; }
    public Location? DefaultDestLocation { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<StockPicking> Pickings { get; set; } = new List<StockPicking>();
}
