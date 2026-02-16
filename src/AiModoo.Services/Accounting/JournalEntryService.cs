using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Accounting;

public class JournalEntryService : IJournalEntryService
{
    private readonly IRepository<JournalEntry> _repository;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IFiscalPeriodService _fiscalPeriodService;
    private readonly IUnitOfWork _unitOfWork;

    public JournalEntryService(
        IRepository<JournalEntry> repository,
        IRepository<Journal> journalRepository,
        IFiscalPeriodService fiscalPeriodService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _journalRepository = journalRepository;
        _fiscalPeriodService = fiscalPeriodService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<JournalEntry>> GetAllAsync(CancellationToken ct = default)
    {
        return await _repository.Query()
            .Include(e => e.Journal)
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.Id)
            .ToListAsync(ct);
    }

    public async Task<JournalEntry?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _repository.Query()
            .Include(e => e.Journal)
            .Include(e => e.FiscalPeriod)
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<JournalEntry> CreateAsync(JournalEntry entry, CancellationToken ct = default)
    {
        _ = await _journalRepository.GetByIdAsync(entry.JournalId, ct)
            ?? throw new NotFoundException(nameof(Journal), entry.JournalId);

        if (entry.Lines == null || entry.Lines.Count < 2)
            throw new BusinessRuleException("MINIMUM_LINES", "Journal entry must have at least 2 lines.");

        ValidateBalance(entry);

        var period = await _fiscalPeriodService.GetPeriodForDateAsync(entry.Date, ct);
        if (period != null)
        {
            if (period.IsClosed) throw new FiscalPeriodClosedException(period.Id);
            entry.FiscalPeriodId = period.Id;
        }

        entry.Number = await GenerateNumberAsync(entry.JournalId, ct);
        entry.Status = JournalEntryStatus.Draft;
        entry.TotalDebit = entry.Lines.Sum(l => l.Debit);
        entry.TotalCredit = entry.Lines.Sum(l => l.Credit);

        await _repository.AddAsync(entry, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return entry;
    }

    public async Task PostAsync(int id, CancellationToken ct = default)
    {
        var entry = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(JournalEntry), id);

        if (entry.Status != JournalEntryStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only draft entries can be posted.");

        ValidateBalance(entry);

        if (entry.FiscalPeriodId.HasValue && entry.FiscalPeriod is { IsClosed: true })
            throw new FiscalPeriodClosedException(entry.FiscalPeriod.Id);

        entry.Status = JournalEntryStatus.Posted;
        entry.PostedAt = DateTime.UtcNow;

        _repository.Update(entry);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var entry = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(JournalEntry), id);

        entry.Status = JournalEntryStatus.Cancelled;
        _repository.Update(entry);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<JournalEntry> ReverseAsync(int id, DateTime reversalDate, CancellationToken ct = default)
    {
        var original = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(JournalEntry), id);

        if (original.Status != JournalEntryStatus.Posted)
            throw new BusinessRuleException("INVALID_STATUS", "Only posted entries can be reversed.");

        var reversal = new JournalEntry
        {
            JournalId = original.JournalId,
            Date = reversalDate,
            Reference = $"Reversal of {original.Number}",
            Narration = $"Reversal of {original.Number}",
            NarrationAr = $"عكس القيد {original.Number}",
            ReversalOfId = original.Id,
            SourceDocument = original.SourceDocument,
            SourceModule = original.SourceModule,
            Lines = original.Lines.Select(l => new JournalEntryLine
            {
                AccountId = l.AccountId,
                Debit = l.Credit,
                Credit = l.Debit,
                PartnerId = l.PartnerId,
                PartnerType = l.PartnerType,
                Label = $"Reversal: {l.Label}",
                CurrencyId = l.CurrencyId,
                AmountCurrency = l.AmountCurrency.HasValue ? -l.AmountCurrency.Value : null
            }).ToList()
        };

        var created = await CreateAsync(reversal, ct);
        await PostAsync(created.Id, ct);
        return created;
    }

    public async Task<string> GenerateNumberAsync(int journalId, CancellationToken ct = default)
    {
        var journal = await _journalRepository.GetByIdAsync(journalId, ct)
            ?? throw new NotFoundException(nameof(Journal), journalId);

        var prefix = journal.SequencePrefix ?? journal.Code;
        var year = DateTime.Now.Year;
        var number = journal.NextSequenceNumber;

        journal.NextSequenceNumber++;
        _journalRepository.Update(journal);

        return $"{prefix}-{year}-{number:D5}";
    }

    private static void ValidateBalance(JournalEntry entry)
    {
        var totalDebit = entry.Lines.Sum(l => l.Debit);
        var totalCredit = entry.Lines.Sum(l => l.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.001m)
            throw new UnbalancedEntryException(totalDebit, totalCredit);
    }
}
