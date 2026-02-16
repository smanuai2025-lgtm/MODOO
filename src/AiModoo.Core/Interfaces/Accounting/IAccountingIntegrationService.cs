using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IAccountingIntegrationService
{
    Task<JournalEntry> PostSalesInvoiceAsync(int invoiceId, CancellationToken ct = default);
    Task<JournalEntry> PostPurchaseInvoiceAsync(int invoiceId, CancellationToken ct = default);
    Task<JournalEntry> PostPaymentAsync(int paymentId, CancellationToken ct = default);
    Task<JournalEntry> PostInventoryMoveAsync(IEnumerable<int> stockMoveIds, CancellationToken ct = default);
    Task<JournalEntry> PostInventoryAdjustmentAsync(int adjustmentId, CancellationToken ct = default);
    Task<JournalEntry> PostPosSessionAsync(int sessionId, CancellationToken ct = default);
    Task<JournalEntry> PostPayrollEntryAsync(int payslipId, CancellationToken ct = default);
    Task ReverseEntryAsync(int journalEntryId, string reason, CancellationToken ct = default);
}
