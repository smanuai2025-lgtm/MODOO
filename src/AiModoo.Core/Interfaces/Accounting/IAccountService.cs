using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IAccountService
{
    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct = default);
    Task<Account?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Account?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetChartOfAccountsTreeAsync(CancellationToken ct = default);
    Task<Account> CreateAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<decimal> GetBalanceAsync(int accountId, DateTime? asOfDate = null, CancellationToken ct = default);
}
