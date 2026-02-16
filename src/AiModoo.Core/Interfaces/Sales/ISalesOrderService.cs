using AiModoo.Core.Entities.Sales;

namespace AiModoo.Core.Interfaces.Sales;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrder>> GetAllAsync(CancellationToken ct = default);
    Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SalesOrder> CreateAsync(SalesOrder order, CancellationToken ct = default);
    Task UpdateAsync(SalesOrder order, CancellationToken ct = default);
    Task ConfirmAsync(int orderId, CancellationToken ct = default);
    Task CreateDeliveryAsync(int orderId, CancellationToken ct = default);
    Task CreateInvoiceAsync(int orderId, CancellationToken ct = default);
    Task CancelAsync(int orderId, CancellationToken ct = default);
    Task ComputeTotalsAsync(SalesOrder order, CancellationToken ct = default);
}
