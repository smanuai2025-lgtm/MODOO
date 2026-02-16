using AiModoo.Core.DTOs.Inventory;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Interfaces.Inventory;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(ProductType? type = null, int? categoryId = null, string? search = null, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ProductCategory> CreateCategoryAsync(ProductCategory category, CancellationToken ct = default);
}
