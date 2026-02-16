using AiModoo.Core.Entities.Inventory;

namespace AiModoo.Core.Interfaces.Inventory;

public interface IInventoryAdjustmentService
{
    Task<IReadOnlyList<InventoryAdjustment>> GetAllAsync(CancellationToken ct = default);
    Task<InventoryAdjustment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<InventoryAdjustment> CreateAsync(InventoryAdjustment adjustment, CancellationToken ct = default);
    Task StartCountAsync(int id, CancellationToken ct = default);
    Task ValidateAsync(int id, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
}
