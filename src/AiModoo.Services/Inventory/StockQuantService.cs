using AiModoo.Core.DTOs.Inventory;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Inventory;

public class StockQuantService : IStockQuantService
{
    private readonly IRepository<StockQuant> _quantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockQuantService(
        IRepository<StockQuant> quantRepository,
        IUnitOfWork unitOfWork)
    {
        _quantRepository = quantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<StockReportDto>> GetStockReportAsync(int? warehouseId = null, int? productId = null, CancellationToken ct = default)
    {
        var query = _quantRepository.Query()
            .Include(q => q.Product).ThenInclude(p => p.Category)
            .Include(q => q.Product).ThenInclude(p => p.Uom)
            .Include(q => q.Location).ThenInclude(l => l!.Warehouse)
            .Where(q => q.Location.LocationType == LocationType.Internal && q.Quantity != 0)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(q => q.Location.WarehouseId == warehouseId.Value);

        if (productId.HasValue)
            query = query.Where(q => q.ProductId == productId.Value);

        var quants = await query.ToListAsync(ct);

        return quants.Select(q => new StockReportDto
        {
            ProductId = q.ProductId,
            ProductName = q.Product.Name,
            ProductNameAr = q.Product.NameAr,
            InternalReference = q.Product.InternalReference,
            CategoryName = q.Product.Category?.Name,
            UomName = q.Product.Uom?.Name,
            OnHand = q.Quantity,
            Reserved = q.ReservedQuantity,
            Available = q.Quantity - q.ReservedQuantity,
            UnitCost = q.Product.Cost,
            TotalValue = q.Quantity * q.Product.Cost,
            WarehouseName = q.Location.Warehouse?.Name,
            LocationName = q.Location.CompleteName
        }).ToList();
    }

    public async Task<decimal> GetOnHandAsync(int productId, int? locationId = null, CancellationToken ct = default)
    {
        var query = _quantRepository.Query()
            .Where(q => q.ProductId == productId);

        if (locationId.HasValue)
            query = query.Where(q => q.LocationId == locationId.Value);
        else
            query = query.Include(q => q.Location).Where(q => q.Location.LocationType == LocationType.Internal);

        return await query.SumAsync(q => q.Quantity, ct);
    }

    public async Task<decimal> GetAvailableAsync(int productId, int? locationId = null, CancellationToken ct = default)
    {
        var query = _quantRepository.Query()
            .Where(q => q.ProductId == productId);

        if (locationId.HasValue)
            query = query.Where(q => q.LocationId == locationId.Value);
        else
            query = query.Include(q => q.Location).Where(q => q.Location.LocationType == LocationType.Internal);

        var quants = await query.ToListAsync(ct);
        return quants.Sum(q => q.Quantity - q.ReservedQuantity);
    }

    public async Task UpdateQuantAsync(int productId, int locationId, decimal quantityChange, int? lotId = null, CancellationToken ct = default)
    {
        var quant = await _quantRepository.Query()
            .FirstOrDefaultAsync(q => q.ProductId == productId && q.LocationId == locationId && q.LotId == lotId, ct);

        if (quant != null)
        {
            quant.Quantity += quantityChange;
            if (quant.Quantity == 0 && quant.ReservedQuantity == 0)
                _quantRepository.Delete(quant);
            else
                _quantRepository.Update(quant);
        }
        else if (quantityChange != 0)
        {
            quant = new StockQuant
            {
                ProductId = productId,
                LocationId = locationId,
                LotId = lotId,
                Quantity = quantityChange,
                InDate = DateTime.UtcNow
            };
            await _quantRepository.AddAsync(quant, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
