using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Inventory;

public class Warehouse : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public int? StockLocationId { get; set; }
    public Location? StockLocation { get; set; }
    public int? InputLocationId { get; set; }
    public Location? InputLocation { get; set; }
    public int? OutputLocationId { get; set; }
    public Location? OutputLocation { get; set; }

    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<StockPickingType> PickingTypes { get; set; } = new List<StockPickingType>();
    public ICollection<ReorderingRule> ReorderingRules { get; set; } = new List<ReorderingRule>();
}
