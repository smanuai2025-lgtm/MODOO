using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IBankReconciliationService
{
    Task<IReadOnlyList<BankReconciliation>> GetAllAsync(CancellationToken ct = default);
    Task<BankReconciliation?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BankReconciliation> CreateAsync(BankReconciliation reconciliation, CancellationToken ct = default);
    Task<BankReconciliation> CreateFromStatementAsync(int statementId, CancellationToken ct = default);
    Task MatchLineAsync(int reconciliationId, int lineId, int journalEntryLineId, CancellationToken ct = default);
    Task UnmatchLineAsync(int reconciliationId, int lineId, CancellationToken ct = default);
    Task AutoMatchAsync(int reconciliationId, CancellationToken ct = default);
    Task ValidateAsync(int reconciliationId, CancellationToken ct = default);
    Task<IReadOnlyList<JournalEntryLine>> GetUnreconciledLinesAsync(int bankAccountId, CancellationToken ct = default);
}
