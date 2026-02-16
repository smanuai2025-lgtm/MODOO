using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Inventory;

public class UnitOfMeasureCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UnitOfMeasure> UnitOfMeasures { get; set; } = new List<UnitOfMeasure>();
}
