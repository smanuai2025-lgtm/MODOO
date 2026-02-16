using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Sales;

public class PricelistService : IPricelistService
{
    private readonly IRepository<Pricelist> _pricelistRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PricelistService(
        IRepository<Pricelist> pricelistRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _pricelistRepository = pricelistRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Pricelist>> GetAllAsync(CancellationToken ct = default)
    {
        return await _pricelistRepository.Query()
            .Include(p => p.Currency)
            .Include(p => p.Items)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<Pricelist?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _pricelistRepository.Query()
            .Include(p => p.Currency)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .Include(p => p.Items).ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Pricelist> CreateAsync(Pricelist pricelist, CancellationToken ct = default)
    {
        var created = await _pricelistRepository.AddAsync(pricelist, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    public async Task UpdateAsync(Pricelist pricelist, CancellationToken ct = default)
    {
        _pricelistRepository.Update(pricelist);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var pricelist = await _pricelistRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Pricelist {id} not found.");
        _pricelistRepository.Delete(pricelist);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<decimal> GetProductPriceAsync(int pricelistId, int productId, decimal qty = 1, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct)
            ?? throw new NotFoundException($"Product {productId} not found.");

        var pricelist = await _pricelistRepository.Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == pricelistId, ct);

        if (pricelist == null) return product.SalePrice;

        var now = DateTime.UtcNow;
        var matchingItem = pricelist.Items
            .Where(i => (i.ProductId == productId || i.CategoryId == product.CategoryId) &&
                        i.MinQuantity <= qty &&
                        (!i.DateStart.HasValue || i.DateStart.Value <= now) &&
                        (!i.DateEnd.HasValue || i.DateEnd.Value >= now))
            .OrderByDescending(i => i.MinQuantity)
            .ThenByDescending(i => i.ProductId.HasValue ? 1 : 0)
            .FirstOrDefault();

        if (matchingItem == null) return product.SalePrice;

        return matchingItem.ComputeMethod switch
        {
            PriceComputeMethod.FixedPrice => matchingItem.FixedPrice,
            PriceComputeMethod.Discount => product.SalePrice * (1 - matchingItem.PercentDiscount / 100),
            _ => product.SalePrice
        };
    }
}
