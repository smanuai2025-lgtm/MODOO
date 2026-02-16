using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Inventory;

public class ProductCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public int? ParentCategoryId { get; set; }
    public ProductCategory? ParentCategory { get; set; }
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductCategory> ChildCategories { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
