using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Inventory;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductCategory> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IRepository<Product> productRepository,
        IRepository<ProductCategory> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(ProductType? type = null, int? categoryId = null, string? search = null, CancellationToken ct = default)
    {
        var query = _productRepository.Query()
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(p => p.Type == type.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.NameAr != null && p.NameAr.Contains(search))
                || (p.InternalReference != null && p.InternalReference.Contains(search))
                || (p.Barcode != null && p.Barcode.Contains(search)));

        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _productRepository.Query()
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .Include(p => p.PurchaseUom)
            .Include(p => p.StockQuants).ThenInclude(q => q.Location)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        return await _productRepository.Query()
            .Include(p => p.Category)
            .Include(p => p.Uom)
            .FirstOrDefaultAsync(p => p.Barcode == barcode, ct);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new BusinessRuleException("PRODUCT_NAME_REQUIRED", "Product name is required.");

        if (!string.IsNullOrWhiteSpace(product.Barcode))
        {
            var exists = await _productRepository.AnyAsync(p => p.Barcode == product.Barcode, ct);
            if (exists)
                throw new BusinessRuleException("BARCODE_EXISTS", $"Barcode '{product.Barcode}' already exists.");
        }

        var created = await _productRepository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var existing = await _productRepository.GetByIdAsync(product.Id, ct)
            ?? throw new NotFoundException($"Product {product.Id} not found.");

        existing.Name = product.Name;
        existing.NameAr = product.NameAr;
        existing.InternalReference = product.InternalReference;
        existing.Barcode = product.Barcode;
        existing.Type = product.Type;
        existing.CategoryId = product.CategoryId;
        existing.UomId = product.UomId;
        existing.PurchaseUomId = product.PurchaseUomId;
        existing.SalePrice = product.SalePrice;
        existing.Cost = product.Cost;
        existing.CostMethod = product.CostMethod;
        existing.Weight = product.Weight;
        existing.Volume = product.Volume;
        existing.TrackingType = product.TrackingType;
        existing.Description = product.Description;
        existing.DescriptionAr = product.DescriptionAr;
        existing.IsActive = product.IsActive;
        existing.CanBeSold = product.CanBeSold;
        existing.CanBePurchased = product.CanBePurchased;
        existing.AccountStockInputId = product.AccountStockInputId;
        existing.AccountStockOutputId = product.AccountStockOutputId;
        existing.AccountExpenseId = product.AccountExpenseId;

        _productRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        _productRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<ProductCategory> CreateCategoryAsync(ProductCategory category, CancellationToken ct = default)
    {
        var created = await _categoryRepository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }
}
