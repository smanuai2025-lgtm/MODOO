using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Inventory;

public class StockPicking : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public StockPickingStatus Status { get; set; } = StockPickingStatus.Draft;

    public int PickingTypeId { get; set; }
    public StockPickingType PickingType { get; set; } = null!;

    public int SourceLocationId { get; set; }
    public Location SourceLocation { get; set; } = null!;

    public int DestLocationId { get; set; }
    public Location DestLocation { get; set; } = null!;

    public int? PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public PartnerType PartnerType { get; set; } = PartnerType.None;

    public DateTime ScheduledDate { get; set; }
    public DateTime? DoneDate { get; set; }

    public string? SourceDocument { get; set; }
    public string? Notes { get; set; }

    public ICollection<StockMove> Moves { get; set; } = new List<StockMove>();
}
