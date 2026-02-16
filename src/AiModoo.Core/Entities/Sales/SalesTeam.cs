using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Sales;

public class SalesTeam : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LeaderUserId { get; set; }

    public ICollection<Partner> Partners { get; set; } = new List<Partner>();
}
