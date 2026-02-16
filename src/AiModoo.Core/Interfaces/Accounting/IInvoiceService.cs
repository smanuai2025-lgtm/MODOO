using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IInvoiceService
{
    Task<IReadOnlyList<Invoice>> GetAllAsync(InvoiceType? type = null, InvoiceStatus? status = null, CancellationToken ct = default);
    Task<Invoice?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Invoice> CreateAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
    Task ConfirmAsync(int id, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
    Task<Invoice> CreateCreditNoteAsync(int invoiceId, CancellationToken ct = default);
    Task RegisterPaymentAsync(int invoiceId, int paymentId, decimal amount, CancellationToken ct = default);
    Task RecalculateAmountsAsync(int invoiceId, CancellationToken ct = default);
    Task<string> GenerateNumberAsync(InvoiceType type, CancellationToken ct = default);
}
