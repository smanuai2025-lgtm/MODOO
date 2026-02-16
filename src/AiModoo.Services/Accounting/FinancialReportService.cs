using AiModoo.Core.DTOs.Accounting;
using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using AccountType = AiModoo.Core.Enums.AccountTypeEnum;

namespace AiModoo.Services.Accounting;

public class FinancialReportService : IFinancialReportService
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<JournalEntryLine> _lineRepository;
    private readonly IRepository<JournalEntry> _entryRepository;
    private readonly IRepository<Partner> _partnerRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Payment> _paymentRepository;

    public FinancialReportService(
        IRepository<Account> accountRepository,
        IRepository<JournalEntryLine> lineRepository,
        IRepository<JournalEntry> entryRepository,
        IRepository<Partner> partnerRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<Payment> paymentRepository)
    {
        _accountRepository = accountRepository;
        _lineRepository = lineRepository;
        _entryRepository = entryRepository;
        _partnerRepository = partnerRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<TrialBalanceReportDto> GetTrialBalanceAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var accounts = await _accountRepository.Query()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(ct);

        var lines = _lineRepository.Query()
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted);

        if (fromDate.HasValue) lines = lines.Where(l => l.JournalEntry.Date >= fromDate.Value);
        if (toDate.HasValue) lines = lines.Where(l => l.JournalEntry.Date <= toDate.Value);

        var grouped = await lines
            .GroupBy(l => l.AccountId)
            .Select(g => new { AccountId = g.Key, Debit = g.Sum(l => l.Debit), Credit = g.Sum(l => l.Credit) })
            .ToListAsync(ct);

        var trialLines = accounts.Select(a =>
        {
            var g = grouped.FirstOrDefault(x => x.AccountId == a.Id);
            var debit = g?.Debit ?? 0;
            var credit = g?.Credit ?? 0;
            var balance = debit - credit;
            return new TrialBalanceDto
            {
                AccountId = a.Id,
                AccountCode = a.Code,
                AccountName = a.Name,
                AccountNameAr = a.NameAr,
                AccountType = a.AccountType.ToString(),
                Debit = debit,
                Credit = credit,
                Balance = balance,
                DebitBalance = balance > 0 ? balance : 0,
                CreditBalance = balance < 0 ? Math.Abs(balance) : 0
            };
        }).Where(l => l.Debit != 0 || l.Credit != 0).ToList();

        return new TrialBalanceReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Lines = trialLines,
            TotalDebit = trialLines.Sum(l => l.Debit),
            TotalCredit = trialLines.Sum(l => l.Credit)
        };
    }

    public async Task<FinancialReportDto> GetIncomeStatementAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var report = new FinancialReportDto
        {
            ReportName = "Income Statement / قائمة الدخل",
            ReportDate = DateTime.Now,
            StartDate = startDate,
            EndDate = endDate,
            Sections = new List<FinancialReportSection>()
        };

        var revenueSection = await BuildSectionAsync("Revenue", "الإيرادات", AccountType.Revenue, startDate, endDate, ct);
        report.Sections.Add(revenueSection);

        var expenseSection = await BuildSectionAsync("Expenses", "المصروفات", AccountType.Expense, startDate, endDate, ct);
        report.Sections.Add(expenseSection);

        report.GrandTotal = revenueSection.SectionTotal - expenseSection.SectionTotal;
        return report;
    }

    public async Task<FinancialReportDto> GetBalanceSheetAsync(DateTime asOfDate, CancellationToken ct = default)
    {
        var report = new FinancialReportDto
        {
            ReportName = "Balance Sheet / الميزانية العمومية",
            ReportDate = asOfDate,
            EndDate = asOfDate,
            Sections = new List<FinancialReportSection>()
        };

        report.Sections.Add(await BuildSectionAsync("Assets", "الأصول", AccountType.Asset, null, asOfDate, ct));
        report.Sections.Add(await BuildSectionAsync("Liabilities", "الالتزامات", AccountType.Liability, null, asOfDate, ct));
        report.Sections.Add(await BuildSectionAsync("Equity", "حقوق الملكية", AccountType.Equity, null, asOfDate, ct));

        report.GrandTotal = report.Sections[0].SectionTotal;
        return report;
    }

    public async Task<List<GeneralLedgerDto>> GetGeneralLedgerAsync(int accountId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        var lines = await _lineRepository.Query()
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId
                && l.JournalEntry.Status == JournalEntryStatus.Posted
                && l.JournalEntry.Date >= fromDate
                && l.JournalEntry.Date <= toDate)
            .OrderBy(l => l.JournalEntry.Date)
            .ThenBy(l => l.JournalEntry.Number)
            .ToListAsync(ct);

        decimal runningBalance = 0;
        return lines.Select(l =>
        {
            runningBalance += l.Debit - l.Credit;
            return new GeneralLedgerDto
            {
                AccountId = accountId,
                AccountCode = account?.Code ?? "",
                AccountName = account?.Name ?? "",
                Date = l.JournalEntry.Date,
                JournalEntryNumber = l.JournalEntry.Number,
                Reference = l.JournalEntry.Reference,
                Label = l.Label ?? l.JournalEntry.Narration,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = runningBalance
            };
        }).ToList();
    }

    public async Task<List<AgedBalanceDto>> GetAgedReceivableAsync(DateTime asOfDate, CancellationToken ct = default)
        => await GetAgedBalanceAsync(AccountType.Asset, asOfDate, ct);

    public async Task<List<AgedBalanceDto>> GetAgedPayableAsync(DateTime asOfDate, CancellationToken ct = default)
        => await GetAgedBalanceAsync(AccountType.Liability, asOfDate, ct);

    private async Task<List<AgedBalanceDto>> GetAgedBalanceAsync(AccountType accountType, DateTime asOfDate, CancellationToken ct)
    {
        var accounts = await _accountRepository.Query()
            .Where(a => a.AccountType == accountType && a.IsReconcilable && a.IsActive)
            .Select(a => a.Id)
            .ToListAsync(ct);

        var unreconciledLines = await _lineRepository.Query()
            .Include(l => l.JournalEntry)
            .Where(l => accounts.Contains(l.AccountId)
                && l.JournalEntry.Status == JournalEntryStatus.Posted
                && !l.IsReconciled
                && l.JournalEntry.Date <= asOfDate
                && l.PartnerId.HasValue)
            .ToListAsync(ct);

        return unreconciledLines
            .GroupBy(l => new { l.PartnerId, l.PartnerType })
            .Select(g =>
            {
                var amount = g.Sum(l => l.Debit - l.Credit);
                var oldestDate = g.Min(l => l.JournalEntry.Date);
                var days = (asOfDate - oldestDate).Days;
                return new AgedBalanceDto
                {
                    PartnerId = g.Key.PartnerId ?? 0,
                    PartnerName = g.Key.PartnerType.ToString(),
                    Total = Math.Abs(amount),
                    Current = days <= 0 ? Math.Abs(amount) : 0,
                    Days1To30 = days > 0 && days <= 30 ? Math.Abs(amount) : 0,
                    Days31To60 = days > 30 && days <= 60 ? Math.Abs(amount) : 0,
                    Days61To90 = days > 60 && days <= 90 ? Math.Abs(amount) : 0,
                    Over90 = days > 90 ? Math.Abs(amount) : 0
                };
            })
            .Where(a => a.Total > 0)
            .OrderByDescending(a => a.Total)
            .ToList();
    }

    private async Task<FinancialReportSection> BuildSectionAsync(string title, string titleAr, AccountType accountType, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var accts = await _accountRepository.Query()
            .Where(a => a.AccountType == accountType && a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(ct);

        var accountIds = accts.Select(a => a.Id).ToList();
        var lines = _lineRepository.Query()
            .Include(l => l.JournalEntry)
            .Where(l => accountIds.Contains(l.AccountId) && l.JournalEntry.Status == JournalEntryStatus.Posted);

        if (fromDate.HasValue) lines = lines.Where(l => l.JournalEntry.Date >= fromDate.Value);
        if (toDate.HasValue) lines = lines.Where(l => l.JournalEntry.Date <= toDate.Value);

        var grouped = await lines
            .GroupBy(l => l.AccountId)
            .Select(g => new { AccountId = g.Key, Debit = g.Sum(l => l.Debit), Credit = g.Sum(l => l.Credit) })
            .ToListAsync(ct);

        var sectionLines = accts.Select(a =>
        {
            var g = grouped.FirstOrDefault(x => x.AccountId == a.Id);
            var amount = g != null ? (accountType == AccountType.Revenue || accountType == AccountType.Liability || accountType == AccountType.Equity
                ? g.Credit - g.Debit : g.Debit - g.Credit) : 0;
            return new FinancialReportLine
            {
                AccountId = a.Id,
                AccountCode = a.Code,
                AccountName = a.Name,
                AccountNameAr = a.NameAr,
                Amount = amount,
                Level = a.Level
            };
        }).Where(l => l.Amount != 0).ToList();

        return new FinancialReportSection
        {
            Title = title,
            TitleAr = titleAr,
            Lines = sectionLines,
            SectionTotal = sectionLines.Sum(l => l.Amount)
        };
    }

    public async Task<PartnerStatementDto> GetPartnerStatementAsync(int partnerId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var partner = await _partnerRepository.GetByIdAsync(partnerId, ct)
            ?? throw new NotFoundException($"Partner {partnerId} not found.");

        var lines = new List<PartnerStatementLineDto>();

        var invoices = await _invoiceRepository.Query()
            .Where(i => i.PartnerId == partnerId
                && i.Date >= fromDate && i.Date <= toDate
                && i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled)
            .OrderBy(i => i.Date).ThenBy(i => i.Id)
            .ToListAsync(ct);

        foreach (var inv in invoices)
        {
            var docType = inv.InvoiceType switch
            {
                InvoiceType.CustomerInvoice => "فاتورة بيع | Sales Invoice",
                InvoiceType.VendorBill => "فاتورة شراء | Purchase Bill",
                InvoiceType.CustomerCreditNote => "إشعار دائن | Credit Note",
                InvoiceType.VendorDebitNote => "إشعار مدين | Debit Note",
                _ => inv.InvoiceType.ToString()
            };
            var isDebit = inv.InvoiceType == InvoiceType.CustomerInvoice || inv.InvoiceType == InvoiceType.VendorDebitNote;
            lines.Add(new PartnerStatementLineDto
            {
                Date = inv.Date,
                DocumentType = docType,
                DocumentNumber = inv.Number,
                Reference = inv.Reference ?? inv.SourceDocument,
                Description = inv.Notes,
                Debit = isDebit ? inv.Total : 0,
                Credit = !isDebit ? inv.Total : 0
            });
        }

        var payments = await _paymentRepository.Query()
            .Where(p => p.PartnerId == partnerId
                && p.Date >= fromDate && p.Date <= toDate
                && p.Status != PaymentStatus.Draft && p.Status != PaymentStatus.Cancelled)
            .OrderBy(p => p.Date).ThenBy(p => p.Id)
            .ToListAsync(ct);

        foreach (var pay in payments)
        {
            var docType = pay.PaymentType == PaymentType.Inbound ? "تحصيل | Receipt" : "دفعة | Payment";
            var isCredit = (partner.PartnerType == PartnerType.Customer && pay.PaymentType == PaymentType.Inbound)
                || (partner.PartnerType == PartnerType.Vendor && pay.PaymentType == PaymentType.Outbound);
            lines.Add(new PartnerStatementLineDto
            {
                Date = pay.Date,
                DocumentType = docType,
                DocumentNumber = pay.Number,
                Reference = pay.Memo,
                Description = pay.Memo,
                Debit = !isCredit ? pay.Amount : 0,
                Credit = isCredit ? pay.Amount : 0
            });
        }

        lines = lines.OrderBy(l => l.Date).ThenBy(l => l.DocumentNumber).ToList();

        // Opening balance
        var priorInvoices = await _invoiceRepository.Query()
            .Where(i => i.PartnerId == partnerId && i.Date < fromDate
                && i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(ct);
        var priorPayments = await _paymentRepository.Query()
            .Where(p => p.PartnerId == partnerId && p.Date < fromDate
                && p.Status != PaymentStatus.Draft && p.Status != PaymentStatus.Cancelled)
            .ToListAsync(ct);

        decimal openingBalance = 0;
        foreach (var inv in priorInvoices)
        {
            var isDebit = inv.InvoiceType == InvoiceType.CustomerInvoice || inv.InvoiceType == InvoiceType.VendorDebitNote;
            openingBalance += isDebit ? inv.Total : -inv.Total;
        }
        foreach (var pay in priorPayments)
        {
            var isCredit = (partner.PartnerType == PartnerType.Customer && pay.PaymentType == PaymentType.Inbound)
                || (partner.PartnerType == PartnerType.Vendor && pay.PaymentType == PaymentType.Outbound);
            openingBalance += isCredit ? -pay.Amount : pay.Amount;
        }

        decimal running = openingBalance;
        foreach (var line in lines)
        {
            running += line.Debit - line.Credit;
            line.RunningBalance = running;
        }

        return new PartnerStatementDto
        {
            PartnerId = partner.Id,
            PartnerName = partner.Name,
            PartnerNameAr = partner.NameAr,
            PartnerType = partner.PartnerType == PartnerType.Customer ? "Customer" : "Vendor",
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            TotalDebit = lines.Sum(l => l.Debit),
            TotalCredit = lines.Sum(l => l.Credit),
            ClosingBalance = running,
            Lines = lines
        };
    }

    public async Task<AccountStatementDto> GetAccountStatementAsync(int accountId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct)
            ?? throw new NotFoundException($"Account {accountId} not found.");

        var priorLines = await _lineRepository.Query()
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId
                && l.JournalEntry.Status == JournalEntryStatus.Posted
                && l.JournalEntry.Date < fromDate)
            .ToListAsync(ct);

        decimal openingBalance = priorLines.Sum(l => l.Debit - l.Credit);

        var periodData = await _lineRepository.Query()
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId
                && l.JournalEntry.Status == JournalEntryStatus.Posted
                && l.JournalEntry.Date >= fromDate && l.JournalEntry.Date <= toDate)
            .OrderBy(l => l.JournalEntry.Date).ThenBy(l => l.JournalEntry.Id)
            .Select(l => new
            {
                l.JournalEntry.Date,
                l.JournalEntry.Number,
                l.JournalEntry.Reference,
                Label = l.Label ?? l.JournalEntry.Narration,
                l.JournalEntry.SourceModule,
                l.PartnerId,
                l.Debit,
                l.Credit
            })
            .ToListAsync(ct);

        var partnerIds = periodData.Where(l => l.PartnerId.HasValue).Select(l => l.PartnerId!.Value).Distinct().ToList();
        var partners = partnerIds.Any()
            ? await _partnerRepository.Query().Where(p => partnerIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name, ct)
            : new Dictionary<int, string>();

        var periodLines = periodData.Select(l => new AccountStatementLineDto
        {
            Date = l.Date,
            JournalEntryNumber = l.Number,
            Reference = l.Reference,
            Label = l.Label,
            PartnerName = l.PartnerId.HasValue && partners.ContainsKey(l.PartnerId.Value) ? partners[l.PartnerId.Value] : null,
            SourceModule = l.SourceModule,
            Debit = l.Debit,
            Credit = l.Credit
        }).ToList();

        decimal running = openingBalance;
        foreach (var line in periodLines)
        {
            running += line.Debit - line.Credit;
            line.RunningBalance = running;
        }

        return new AccountStatementDto
        {
            AccountId = account.Id,
            AccountCode = account.Code,
            AccountName = account.Name,
            AccountNameAr = account.NameAr,
            AccountType = account.AccountType.ToString(),
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            TotalDebit = periodLines.Sum(l => l.Debit),
            TotalCredit = periodLines.Sum(l => l.Credit),
            ClosingBalance = running,
            Lines = periodLines
        };
    }
}
