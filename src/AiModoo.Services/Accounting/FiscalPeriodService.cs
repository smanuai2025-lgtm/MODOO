using AiModoo.Core.Constants;
using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiModoo.Services.Accounting;

public class FiscalPeriodService : IFiscalPeriodService
{
    private readonly IRepository<FiscalYear> _yearRepository;
    private readonly IRepository<FiscalPeriod> _periodRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<JournalEntryLine> _journalLineRepository;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWork _unitOfWork;

    public FiscalPeriodService(
        IRepository<FiscalYear> yearRepository,
        IRepository<FiscalPeriod> periodRepository,
        IRepository<Account> accountRepository,
        IRepository<JournalEntryLine> journalLineRepository,
        IRepository<Journal> journalRepository,
        IServiceProvider serviceProvider,
        IUnitOfWork unitOfWork)
    {
        _yearRepository = yearRepository;
        _periodRepository = periodRepository;
        _accountRepository = accountRepository;
        _journalLineRepository = journalLineRepository;
        _journalRepository = journalRepository;
        _serviceProvider = serviceProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<FiscalYear>> GetAllYearsAsync(CancellationToken ct = default)
    {
        return await _yearRepository.Query().Include(y => y.Periods).OrderByDescending(y => y.StartDate).ToListAsync(ct);
    }

    public async Task<FiscalYear> CreateYearAsync(FiscalYear year, bool generatePeriods = true, CancellationToken ct = default)
    {
        await _yearRepository.AddAsync(year, ct);

        if (generatePeriods)
        {
            var currentDate = year.StartDate;
            int periodNum = 1;
            while (currentDate < year.EndDate)
            {
                var periodEnd = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                if (periodEnd > year.EndDate) periodEnd = year.EndDate;

                year.Periods.Add(new FiscalPeriod
                {
                    Name = $"Period {periodNum:D2} - {currentDate:MMM yyyy}",
                    StartDate = currentDate,
                    EndDate = periodEnd
                });

                currentDate = periodEnd.AddDays(1);
                periodNum++;
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return year;
    }

    public async Task<FiscalPeriod?> GetCurrentPeriodAsync(CancellationToken ct = default)
    {
        return await GetPeriodForDateAsync(DateTime.Today, ct);
    }

    public async Task<FiscalPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken ct = default)
    {
        return await _periodRepository.Query()
            .FirstOrDefaultAsync(p => p.StartDate <= date && p.EndDate >= date && !p.IsClosed, ct);
    }

    public async Task ClosePeriodAsync(int periodId, CancellationToken ct = default)
    {
        var period = await _periodRepository.GetByIdAsync(periodId, ct)
            ?? throw new NotFoundException(nameof(FiscalPeriod), periodId);
        period.IsClosed = true;
        _periodRepository.Update(period);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CloseYearAsync(int yearId, CancellationToken ct = default)
    {
        var year = await _yearRepository.Query().Include(y => y.Periods)
            .FirstOrDefaultAsync(y => y.Id == yearId, ct)
            ?? throw new NotFoundException(nameof(FiscalYear), yearId);

        foreach (var period in year.Periods) period.IsClosed = true;
        year.IsClosed = true;
        _yearRepository.Update(year);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<JournalEntry> GenerateClosingEntryAsync(int yearId, CancellationToken ct = default)
    {
        var year = await _yearRepository.Query()
            .Include(y => y.Periods)
            .FirstOrDefaultAsync(y => y.Id == yearId, ct)
            ?? throw new NotFoundException(nameof(FiscalYear), yearId);

        if (!year.IsClosed && year.Periods.Any(p => !p.IsClosed))
            throw new BusinessRuleException("YEAR_NOT_CLOSED", "Fiscal year must be closed or all periods must be closed before generating closing entry.");

        // Get all posted journal entry lines within the fiscal year date range
        var postedLines = await _journalLineRepository.Query()
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted
                && l.JournalEntry.Date >= year.StartDate
                && l.JournalEntry.Date <= year.EndDate)
            .ToListAsync(ct);

        // Revenue accounts: AccountType == Revenue
        var revenueLines = postedLines
            .Where(l => l.Account.AccountType == AccountTypeEnum.Revenue)
            .ToList();

        // Expense accounts: AccountType == Expense
        var expenseLines = postedLines
            .Where(l => l.Account.AccountType == AccountTypeEnum.Expense)
            .ToList();

        // Group by account and calculate balances
        var revenueByAccount = revenueLines
            .GroupBy(l => l.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Balance = g.Sum(l => l.Credit) - g.Sum(l => l.Debit) // Revenue balance = credits - debits
            })
            .Where(a => a.Balance != 0)
            .ToList();

        var expenseByAccount = expenseLines
            .GroupBy(l => l.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Balance = g.Sum(l => l.Debit) - g.Sum(l => l.Credit) // Expense balance = debits - credits
            })
            .Where(a => a.Balance != 0)
            .ToList();

        var totalRevenue = revenueByAccount.Sum(a => a.Balance);
        var totalExpenses = expenseByAccount.Sum(a => a.Balance);
        var netIncome = totalRevenue - totalExpenses;

        // Find the General journal (code "GEN")
        var generalJournal = await _journalRepository.Query()
            .FirstOrDefaultAsync(j => j.Code == "GEN", ct)
            ?? throw new BusinessRuleException("JOURNAL_NOT_FOUND", "General journal (GEN) not found.");

        // Find the Retained Earnings account (code "3002")
        var retainedEarningsAccount = await _accountRepository.Query()
            .FirstOrDefaultAsync(a => a.Code == AccountCodes.RetainedEarnings, ct)
            ?? throw new BusinessRuleException("ACCOUNT_NOT_FOUND", $"Retained Earnings account ({AccountCodes.RetainedEarnings}) not found.");

        var closingLines = new List<JournalEntryLine>();

        // For each Revenue account with balance: Debit the revenue account to close it
        foreach (var revenue in revenueByAccount)
        {
            closingLines.Add(new JournalEntryLine
            {
                AccountId = revenue.AccountId,
                Debit = revenue.Balance,
                Credit = 0,
                Label = "Year-end closing - Revenue"
            });
        }

        // For each Expense account with balance: Credit the expense account to close it
        foreach (var expense in expenseByAccount)
        {
            closingLines.Add(new JournalEntryLine
            {
                AccountId = expense.AccountId,
                Debit = 0,
                Credit = expense.Balance,
                Label = "Year-end closing - Expense"
            });
        }

        // Final balancing line: Retained Earnings with net income
        if (netIncome >= 0)
        {
            closingLines.Add(new JournalEntryLine
            {
                AccountId = retainedEarningsAccount.Id,
                Debit = 0,
                Credit = netIncome,
                Label = "Year-end closing - Net Income to Retained Earnings"
            });
        }
        else
        {
            closingLines.Add(new JournalEntryLine
            {
                AccountId = retainedEarningsAccount.Id,
                Debit = Math.Abs(netIncome),
                Credit = 0,
                Label = "Year-end closing - Net Loss to Retained Earnings"
            });
        }

        var closingEntry = new JournalEntry
        {
            JournalId = generalJournal.Id,
            Date = year.EndDate,
            Reference = $"CLOSING-{year.Name}",
            Narration = $"Year-end closing entry for {year.Name}",
            NarrationAr = $"قيد إقفال نهاية السنة {year.Name}",
            SourceDocument = $"FiscalYear-{year.Id}",
            SourceModule = "FiscalPeriod",
            Lines = closingLines
        };

        // Resolve lazily to avoid circular dependency (JournalEntryService -> FiscalPeriodService -> JournalEntryService)
        var journalEntryService = _serviceProvider.GetRequiredService<IJournalEntryService>();
        var createdEntry = await journalEntryService.CreateAsync(closingEntry, ct);
        await journalEntryService.PostAsync(createdEntry.Id, ct);

        return createdEntry;
    }
}
