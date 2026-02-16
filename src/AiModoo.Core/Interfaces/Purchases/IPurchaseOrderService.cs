using AiModoo.Core.Entities.Purchases;

namespace AiModoo.Core.Interfaces.Purchases;

public interface IPurchaseOrderService
{
    Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken ct = default);
    Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PurchaseOrder> CreateAsync(PurchaseOrder order, CancellationToken ct = default);
    Task UpdateAsync(PurchaseOrder order, CancellationToken ct = default);
    Task ConfirmAsync(int orderId, CancellationToken ct = default);
    Task CreateReceiptAsync(int orderId, CancellationToken ct = default);
    Task CreateBillAsync(int orderId, CancellationToken ct = default);
    Task CancelAsync(int orderId, CancellationToken ct = default);
    Task ComputeTotalsAsync(PurchaseOrder order, CancellationToken ct = default);
}
