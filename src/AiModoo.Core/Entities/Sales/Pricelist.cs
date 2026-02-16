using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Sales;

public class Pricelist : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PricelistItem> Items { get; set; } = new List<PricelistItem>();
}

public class PricelistItem : BaseEntity
{
    public int PricelistId { get; set; }
    public Pricelist Pricelist { get; set; } = null!;

    public int? ProductId { get; set; }
    public Inventory.Product? Product { get; set; }
    public int? CategoryId { get; set; }
    public Inventory.ProductCategory? Category { get; set; }

    public PriceComputeMethod ComputeMethod { get; set; } = PriceComputeMethod.FixedPrice;
    public decimal FixedPrice { get; set; }
    public decimal PercentDiscount { get; set; }
    public int MinQuantity { get; set; } = 1;

    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
}
