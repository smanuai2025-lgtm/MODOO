using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Accounting;

public class TaxService : ITaxService
{
    private readonly IRepository<Tax> _repository;
    private readonly IRepository<TaxGroup> _groupRepository;
    private readonly IRepository<TaxGroupDetail> _groupDetailRepository;
    private readonly IRepository<JournalEntryLine> _journalLineRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaxService(
        IRepository<Tax> repository,
        IRepository<TaxGroup> groupRepository,
        IRepository<TaxGroupDetail> groupDetailRepository,
        IRepository<JournalEntryLine> journalLineRepository,
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _groupRepository = groupRepository;
        _groupDetailRepository = groupDetailRepository;
        _journalLineRepository = journalLineRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Tax>> GetAllActiveAsync(CancellationToken ct = default)
        => await _repository.Query().Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(ct);

    public async Task<Tax?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repository.GetByIdAsync(id, ct);

    public async Task<Tax> CreateAsync(Tax tax, CancellationToken ct = default)
    {
        await _repository.AddAsync(tax, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return tax;
    }

    public async Task UpdateAsync(Tax tax, CancellationToken ct = default)
    {
        _repository.Update(tax);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<decimal> CalculateTaxAmount(decimal amount, int taxId, CancellationToken ct = default)
    {
        var tax = await _repository.GetByIdAsync(taxId, ct)
            ?? throw new NotFoundException(nameof(Tax), taxId);

        return tax.Type switch
        {
            TaxType.Percentage => Math.Round(amount * tax.Rate / 100, 2),
            TaxType.Fixed => tax.Rate,
            _ => 0
        };
    }

    // Tax Groups
    public async Task<IReadOnlyList<TaxGroup>> GetAllGroupsAsync(CancellationToken ct = default)
        => await _groupRepository.Query()
            .Include(g => g.Details).ThenInclude(d => d.Tax)
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

    public async Task<TaxGroup?> GetGroupByIdAsync(int id, CancellationToken ct = default)
        => await _groupRepository.Query()
            .Include(g => g.Details).ThenInclude(d => d.Tax)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<TaxGroup> CreateGroupAsync(TaxGroup group, CancellationToken ct = default)
    {
        if (group.Details == null || group.Details.Count == 0)
            throw new BusinessRuleException("NO_TAXES", "Tax group must have at least one tax.");

        await _groupRepository.AddAsync(group, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return group;
    }

    public async Task<decimal> CalculateGroupTaxAmount(decimal amount, int groupId, CancellationToken ct = default)
    {
        var group = await GetGroupByIdAsync(groupId, ct)
            ?? throw new NotFoundException(nameof(TaxGroup), groupId);

        decimal totalTax = 0;
        foreach (var detail in group.Details.OrderBy(d => d.Sequence))
        {
            totalTax += await CalculateTaxAmount(amount, detail.TaxId, ct);
        }
        return totalTax;
    }

    // Tax Report
    public async Task<TaxReportResult> GetTaxReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        // Get all tax accounts
        var taxes = await _repository.Query()
            .Include(t => t.SalesAccount)
            .Include(t => t.PurchaseAccount)
            .Where(t => t.IsActive)
            .ToListAsync(ct);

        var result = new TaxReportResult
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        foreach (var tax in taxes)
        {
            // Sales tax
            if (tax.SalesAccountId.HasValue)
            {
                var salesLines = await _journalLineRepository.Query()
                    .Include(l => l.JournalEntry)
                    .Where(l => l.AccountId == tax.SalesAccountId.Value
                        && l.JournalEntry.Status == JournalEntryStatus.Posted
                        && l.JournalEntry.Date >= fromDate
                        && l.JournalEntry.Date <= toDate)
                    .ToListAsync(ct);

                var salesTaxAmount = salesLines.Sum(l => l.Credit) - salesLines.Sum(l => l.Debit);

                if (salesTaxAmount != 0)
                {
                    var taxableAmount = tax.Rate > 0 ? Math.Round(salesTaxAmount / (tax.Rate / 100), 2) : 0;
                    result.Lines.Add(new TaxReportLine
                    {
                        TaxName = tax.Name,
                        Rate = tax.Rate,
                        TaxableAmount = taxableAmount,
                        TaxAmount = salesTaxAmount,
                        Type = "Sales"
                    });
                    result.TotalSalesTax += salesTaxAmount;
                }
            }

            // Purchase tax
            if (tax.PurchaseAccountId.HasValue)
            {
                var purchaseLines = await _journalLineRepository.Query()
                    .Include(l => l.JournalEntry)
                    .Where(l => l.AccountId == tax.PurchaseAccountId.Value
                        && l.JournalEntry.Status == JournalEntryStatus.Posted
                        && l.JournalEntry.Date >= fromDate
                        && l.JournalEntry.Date <= toDate)
                    .ToListAsync(ct);

                var purchaseTaxAmount = purchaseLines.Sum(l => l.Debit) - purchaseLines.Sum(l => l.Credit);

                if (purchaseTaxAmount != 0)
                {
                    var taxableAmount = tax.Rate > 0 ? Math.Round(purchaseTaxAmount / (tax.Rate / 100), 2) : 0;
                    result.Lines.Add(new TaxReportLine
                    {
                        TaxName = tax.Name,
                        Rate = tax.Rate,
                        TaxableAmount = taxableAmount,
                        TaxAmount = purchaseTaxAmount,
                        Type = "Purchase"
                    });
                    result.TotalPurchaseTax += purchaseTaxAmount;
                }
            }
        }

        result.NetTax = result.TotalSalesTax - result.TotalPurchaseTax;
        return result;
    }
}
