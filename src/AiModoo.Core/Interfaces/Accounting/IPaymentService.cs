using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IPaymentService
{
    Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken ct = default);
    Task PostAsync(int id, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
    Task AllocateToInvoiceAsync(int paymentId, int invoiceId, decimal amount, CancellationToken ct = default);
    Task DeallocateFromInvoiceAsync(int allocationId, CancellationToken ct = default);
}
