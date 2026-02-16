using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Accounting;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _repository;
    private readonly IRepository<JournalEntryLine> _lineRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(IRepository<Account> repository, IRepository<JournalEntryLine> lineRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct = default)
    {
        return await _repository.Query()
            .Include(a => a.ParentAccount)
            .OrderBy(a => a.Code)
            .ToListAsync(ct);
    }

    public async Task<Account?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _repository.Query()
            .Include(a => a.ParentAccount)
            .Include(a => a.ChildAccounts)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<Account?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _repository.Query().FirstOrDefaultAsync(a => a.Code == code, ct);
    }

    public async Task<IReadOnlyList<Account>> GetChartOfAccountsTreeAsync(CancellationToken ct = default)
    {
        return await _repository.Query()
            .Include(a => a.ChildAccounts)
            .Where(a => a.ParentAccountId == null)
            .OrderBy(a => a.Code)
            .ToListAsync(ct);
    }

    public async Task<Account> CreateAsync(Account account, CancellationToken ct = default)
    {
        if (await _repository.AnyAsync(a => a.Code == account.Code, ct))
            throw new BusinessRuleException("DUPLICATE_CODE", $"Account code '{account.Code}' already exists.");

        if (account.ParentAccountId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(account.ParentAccountId.Value, ct)
                ?? throw new NotFoundException(nameof(Account), account.ParentAccountId.Value);
            account.Level = parent.Level + 1;
        }

        await _repository.AddAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return account;
    }

    public async Task UpdateAsync(Account account, CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdAsync(account.Id, ct)
            ?? throw new NotFoundException(nameof(Account), account.Id);

        existing.Name = account.Name;
        existing.NameAr = account.NameAr;
        existing.AccountType = account.AccountType;
        existing.IsActive = account.IsActive;
        existing.IsReconcilable = account.IsReconcilable;
        existing.CurrencyId = account.CurrencyId;

        _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Account), id);

        if (await _lineRepository.AnyAsync(l => l.AccountId == id, ct))
            throw new BusinessRuleException("ACCOUNT_IN_USE", "Cannot delete account with existing journal entries.");

        _repository.Delete(account);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<decimal> GetBalanceAsync(int accountId, DateTime? asOfDate = null, CancellationToken ct = default)
    {
        var query = _lineRepository.Query()
            .Where(l => l.AccountId == accountId && l.JournalEntry.Status == Core.Enums.JournalEntryStatus.Posted);

        if (asOfDate.HasValue)
            query = query.Where(l => l.JournalEntry.Date <= asOfDate.Value);

        var debit = await query.SumAsync(l => l.Debit, ct);
        var credit = await query.SumAsync(l => l.Credit, ct);
        return debit - credit;
    }
}
