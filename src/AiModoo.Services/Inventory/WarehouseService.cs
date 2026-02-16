using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Inventory;

public class WarehouseService : IWarehouseService
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseService(
        IRepository<Warehouse> warehouseRepository,
        IRepository<Location> locationRepository,
        IUnitOfWork unitOfWork)
    {
        _warehouseRepository = warehouseRepository;
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _warehouseRepository.Query()
            .Include(w => w.Locations)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _warehouseRepository.Query()
            .Include(w => w.Locations)
            .Include(w => w.StockLocation)
            .Include(w => w.InputLocation)
            .Include(w => w.OutputLocation)
            .Include(w => w.PickingTypes)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(warehouse.Code))
            throw new BusinessRuleException("WH_CODE_REQUIRED", "Warehouse code is required.");

        var exists = await _warehouseRepository.AnyAsync(w => w.Code == warehouse.Code, ct);
        if (exists)
            throw new BusinessRuleException("WH_CODE_EXISTS", $"Warehouse code '{warehouse.Code}' already exists.");

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var created = await _warehouseRepository.AddAsync(warehouse, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Create default locations
            var stockLoc = new Location
            {
                Name = $"{warehouse.Name}/Stock",
                NameAr = warehouse.NameAr != null ? $"{warehouse.NameAr}/المخزن" : null,
                CompleteName = $"{warehouse.Name}/Stock",
                LocationType = Core.Enums.LocationType.Internal,
                WarehouseId = created.Id,
                IsActive = true
            };
            var inputLoc = new Location
            {
                Name = $"{warehouse.Name}/Input",
                NameAr = warehouse.NameAr != null ? $"{warehouse.NameAr}/الاستلام" : null,
                CompleteName = $"{warehouse.Name}/Input",
                LocationType = Core.Enums.LocationType.Internal,
                WarehouseId = created.Id,
                IsActive = true
            };
            var outputLoc = new Location
            {
                Name = $"{warehouse.Name}/Output",
                NameAr = warehouse.NameAr != null ? $"{warehouse.NameAr}/التسليم" : null,
                CompleteName = $"{warehouse.Name}/Output",
                LocationType = Core.Enums.LocationType.Internal,
                WarehouseId = created.Id,
                IsActive = true
            };

            await _locationRepository.AddAsync(stockLoc, ct);
            await _locationRepository.AddAsync(inputLoc, ct);
            await _locationRepository.AddAsync(outputLoc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            created.StockLocationId = stockLoc.Id;
            created.InputLocationId = inputLoc.Id;
            created.OutputLocationId = outputLoc.Id;
            _warehouseRepository.Update(created);
            await _unitOfWork.SaveChangesAsync(ct);

            await _unitOfWork.CommitTransactionAsync(ct);
            return created;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task UpdateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        var existing = await _warehouseRepository.GetByIdAsync(warehouse.Id, ct)
            ?? throw new NotFoundException($"Warehouse {warehouse.Id} not found.");

        existing.Name = warehouse.Name;
        existing.NameAr = warehouse.NameAr;
        existing.Code = warehouse.Code;
        existing.Address = warehouse.Address;
        existing.IsActive = warehouse.IsActive;

        _warehouseRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Location>> GetLocationsAsync(int? warehouseId = null, CancellationToken ct = default)
    {
        var query = _locationRepository.Query()
            .Include(l => l.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(l => l.WarehouseId == warehouseId.Value);

        return await query.OrderBy(l => l.CompleteName).ToListAsync(ct);
    }

    public async Task<Location> CreateLocationAsync(Location location, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(location.CompleteName))
            location.CompleteName = location.Name;

        var created = await _locationRepository.AddAsync(location, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }
}
