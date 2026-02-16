using AiModoo.Core.Enums;

namespace AiModoo.Core.DTOs.Inventory;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? InternalReference { get; set; }
    public string? Barcode { get; set; }
    public ProductType Type { get; set; }
    public string? CategoryName { get; set; }
    public string? UomName { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Cost { get; set; }
    public decimal OnHand { get; set; }
    public decimal Available { get; set; }
    public bool IsActive { get; set; }
}
