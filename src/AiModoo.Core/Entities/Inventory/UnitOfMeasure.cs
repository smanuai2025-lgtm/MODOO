using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Inventory;

public class UnitOfMeasure : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public int CategoryId { get; set; }
    public UnitOfMeasureCategory Category { get; set; } = null!;
    public UomType Type { get; set; } = UomType.Reference;
    public decimal Ratio { get; set; } = 1;
    public decimal Rounding { get; set; } = 0.01m;
    public bool IsActive { get; set; } = true;
}
