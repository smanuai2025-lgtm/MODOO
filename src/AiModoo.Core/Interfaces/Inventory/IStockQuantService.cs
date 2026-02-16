using AiModoo.Core.DTOs.Inventory;
using AiModoo.Core.Entities.Inventory;

namespace AiModoo.Core.Interfaces.Inventory;

public interface IStockQuantService
{
    Task<IReadOnlyList<StockReportDto>> GetStockReportAsync(int? warehouseId = null, int? productId = null, CancellationToken ct = default);
    Task<decimal> GetOnHandAsync(int productId, int? locationId = null, CancellationToken ct = default);
    Task<decimal> GetAvailableAsync(int productId, int? locationId = null, CancellationToken ct = default);
    Task UpdateQuantAsync(int productId, int locationId, decimal quantityChange, int? lotId = null, CancellationToken ct = default);
}
