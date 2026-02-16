using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IBankStatementService
{
    Task<IReadOnlyList<BankStatement>> GetAllAsync(CancellationToken ct = default);
    Task<BankStatement?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BankStatement> CreateAsync(BankStatement statement, CancellationToken ct = default);
    Task<BankStatement> ImportCsvAsync(int bankAccountId, Stream csvStream, CancellationToken ct = default);
    Task ProcessAsync(int statementId, CancellationToken ct = default);
    Task<IReadOnlyList<BankStatementLine>> AutoMatchAsync(int statementId, CancellationToken ct = default);
}
