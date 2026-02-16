using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Interfaces.Inventory;

public interface IStockPickingService
{
    Task<IReadOnlyList<StockPicking>> GetAllAsync(PickingTypeCode? typeCode = null, StockPickingStatus? status = null, CancellationToken ct = default);
    Task<StockPicking?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StockPicking> CreateAsync(StockPicking picking, CancellationToken ct = default);
    Task ConfirmAsync(int id, CancellationToken ct = default);
    Task ValidateAsync(int id, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<StockPickingType>> GetPickingTypesAsync(CancellationToken ct = default);
}
