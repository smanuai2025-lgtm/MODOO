using AiModoo.Core.Entities.Inventory;

namespace AiModoo.Core.Interfaces.Inventory;

public interface IWarehouseService
{
    Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken ct = default);
    Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken ct = default);
    Task UpdateAsync(Warehouse warehouse, CancellationToken ct = default);
    Task<IReadOnlyList<Location>> GetLocationsAsync(int? warehouseId = null, CancellationToken ct = default);
    Task<Location> CreateLocationAsync(Location location, CancellationToken ct = default);
}
