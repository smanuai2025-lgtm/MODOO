using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Inventory;

public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? InternalReference { get; set; }
    public string? Barcode { get; set; }
    public ProductType Type { get; set; } = ProductType.Storable;

    public int? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    public int? UomId { get; set; }
    public UnitOfMeasure? Uom { get; set; }

    public int? PurchaseUomId { get; set; }
    public UnitOfMeasure? PurchaseUom { get; set; }

    public decimal SalePrice { get; set; }
    public decimal Cost { get; set; }
    public CostMethod CostMethod { get; set; } = CostMethod.Standard;

    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public TrackingType TrackingType { get; set; } = TrackingType.None;

    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool CanBeSold { get; set; } = true;
    public bool CanBePurchased { get; set; } = true;

    // Accounting integration
    public int? AccountStockInputId { get; set; }
    public Account? AccountStockInput { get; set; }
    public int? AccountStockOutputId { get; set; }
    public Account? AccountStockOutput { get; set; }
    public int? AccountExpenseId { get; set; }
    public Account? AccountExpense { get; set; }

    // Navigation
    public ICollection<StockMove> StockMoves { get; set; } = new List<StockMove>();
    public ICollection<StockQuant> StockQuants { get; set; } = new List<StockQuant>();
    public ICollection<StockLot> StockLots { get; set; } = new List<StockLot>();
    public ICollection<ReorderingRule> ReorderingRules { get; set; } = new List<ReorderingRule>();
}
