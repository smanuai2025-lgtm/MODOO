using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Inventory;

public class Location : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string CompleteName { get; set; } = string.Empty;
    public LocationType LocationType { get; set; } = LocationType.Internal;

    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int? ParentLocationId { get; set; }
    public Location? ParentLocation { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsScrapLocation { get; set; }

    public ICollection<Location> ChildLocations { get; set; } = new List<Location>();
    public ICollection<StockQuant> StockQuants { get; set; } = new List<StockQuant>();
}
