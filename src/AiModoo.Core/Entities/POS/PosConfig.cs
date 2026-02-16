using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.Sales;

namespace AiModoo.Core.Entities.POS;

public class PosConfig : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;

    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int? StockLocationId { get; set; }
    public Location? StockLocation { get; set; }

    public int? JournalId { get; set; }
    public Journal? Journal { get; set; }

    public int? PricelistId { get; set; }
    public Pricelist? Pricelist { get; set; }

    public bool IsTaxIncluded { get; set; } = true;
    public int? DefaultTaxId { get; set; }
    public Tax? DefaultTax { get; set; }

    public decimal OpeningBalance { get; set; }

    public ICollection<PosSession> Sessions { get; set; } = new List<PosSession>();
}
