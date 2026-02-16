using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Accounting;

public class BankReconciliationService : IBankReconciliationService
{
    private readonly IRepository<BankReconciliation> _repository;
    private readonly IRepository<BankReconciliationLine> _lineRepository;
    private readonly IRepository<JournalEntryLine> _journalLineRepository;
    private readonly IRepository<BankStatement> _statementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BankReconciliationService(
        IRepository<BankReconciliation> repository,
        IRepository<BankReconciliationLine> lineRepository,
        IRepository<JournalEntryLine> journalLineRepository,
        IRepository<BankStatement> statementRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _journalLineRepository = journalLineRepository;
        _statementRepository = statementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<BankReconciliation>> GetAllAsync(CancellationToken ct = default)
        => await _repository.Query()
            .Include(r => r.BankAccount)
            .OrderByDescending(r => r.Date)
            .ToListAsync(ct);

    public async Task<BankReconciliation?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repository.Query()
            .Include(r => r.BankAccount)
            .Include(r => r.Lines).ThenInclude(l => l.JournalEntryLine)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<BankReconciliation> CreateAsync(BankReconciliation reconciliation, CancellationToken ct = default)
    {
        reconciliation.Status = ReconciliationStatus.InProgress;
        await _repository.AddAsync(reconciliation, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return reconciliation;
    }

    public async Task MatchLineAsync(int reconciliationId, int lineId, int journalEntryLineId, CancellationToken ct = default)
    {
        var reconciliation = await GetByIdAsync(reconciliationId, ct)
            ?? throw new NotFoundException(nameof(BankReconciliation), reconciliationId);

        if (reconciliation.Status != ReconciliationStatus.InProgress)
            throw new BusinessRuleException("INVALID_STATUS", "Can only match lines in in-progress reconciliations.");

        var line = reconciliation.Lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new NotFoundException(nameof(BankReconciliationLine), lineId);

        line.JournalEntryLineId = journalEntryLineId;
        line.IsMatched = true;
        _lineRepository.Update(line);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UnmatchLineAsync(int reconciliationId, int lineId, CancellationToken ct = default)
    {
        var reconciliation = await GetByIdAsync(reconciliationId, ct)
            ?? throw new NotFoundException(nameof(BankReconciliation), reconciliationId);

        var line = reconciliation.Lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new NotFoundException(nameof(BankReconciliationLine), lineId);

        line.JournalEntryLineId = null;
        line.IsMatched = false;
        _lineRepository.Update(line);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ValidateAsync(int reconciliationId, CancellationToken ct = default)
    {
        var reconciliation = await GetByIdAsync(reconciliationId, ct)
            ?? throw new NotFoundException(nameof(BankReconciliation), reconciliationId);

        if (reconciliation.Status != ReconciliationStatus.InProgress)
            throw new BusinessRuleException("INVALID_STATUS", "Only in-progress reconciliations can be validated.");

        var unmatchedCount = reconciliation.Lines.Count(l => !l.IsMatched);
        if (unmatchedCount > 0)
            throw new BusinessRuleException("UNMATCHED_LINES", $"There are {unmatchedCount} unmatched lines.");

        foreach (var line in reconciliation.Lines.Where(l => l.IsMatched && l.JournalEntryLineId.HasValue))
        {
            var journalLine = await _journalLineRepository.GetByIdAsync(line.JournalEntryLineId!.Value, ct);
            if (journalLine != null)
            {
                journalLine.IsReconciled = true;
                journalLine.ReconcileGroupId = reconciliationId;
                _journalLineRepository.Update(journalLine);
            }
        }

        reconciliation.Status = ReconciliationStatus.Validated;
        _repository.Update(reconciliation);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<JournalEntryLine>> GetUnreconciledLinesAsync(int bankAccountId, CancellationToken ct = default)
    {
        return await _journalLineRepository.Query()
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .Where(l => l.Account.Id == bankAccountId
                && l.JournalEntry.Status == JournalEntryStatus.Posted
                && !l.IsReconciled)
            .OrderBy(l => l.JournalEntry.Date)
            .ToListAsync(ct);
    }

    public async Task<BankReconciliation> CreateFromStatementAsync(int statementId, CancellationToken ct = default)
    {
        var statement = await _statementRepository.Query()
            .Include(s => s.BankAccount)
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == statementId, ct)
            ?? throw new NotFoundException(nameof(BankStatement), statementId);

        var reconciliation = new BankReconciliation
        {
            BankAccountId = statement.BankAccountId,
            Date = statement.Date,
            StartingBalance = statement.OpeningBalance,
            EndingBalance = statement.ClosingBalance,
            Status = ReconciliationStatus.InProgress,
            Lines = new List<BankReconciliationLine>()
        };

        foreach (var statementLine in statement.Lines)
        {
            var reconciliationLine = new BankReconciliationLine
            {
                Date = statementLine.Date,
                Reference = statementLine.Reference,
                Description = statementLine.Description,
                Amount = statementLine.Amount,
                PartnerName = statementLine.PartnerName,
                IsMatched = false
            };

            // Auto-set as matched if the statement line already has a matched journal entry line
            if (statementLine.MatchedJournalEntryLineId.HasValue)
            {
                reconciliationLine.JournalEntryLineId = statementLine.MatchedJournalEntryLineId.Value;
                reconciliationLine.IsMatched = true;
            }

            reconciliation.Lines.Add(reconciliationLine);
        }

        await _repository.AddAsync(reconciliation, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return reconciliation;
    }

    public async Task AutoMatchAsync(int reconciliationId, CancellationToken ct = default)
    {
        var reconciliation = await GetByIdAsync(reconciliationId, ct)
            ?? throw new NotFoundException(nameof(BankReconciliation), reconciliationId);

        if (reconciliation.Status != ReconciliationStatus.InProgress)
            throw new BusinessRuleException("INVALID_STATUS", "Can only auto-match lines in in-progress reconciliations.");

        var unmatchedLines = reconciliation.Lines
            .Where(l => !l.IsMatched)
            .ToList();

        if (unmatchedLines.Count == 0) return;

        // Get unreconciled journal entry lines for the bank account
        var journalLines = await _journalLineRepository.Query()
            .Include(jl => jl.JournalEntry)
            .Include(jl => jl.Account)
            .Where(jl => jl.Account.Id == reconciliation.BankAccountId
                && jl.JournalEntry.Status == JournalEntryStatus.Posted
                && !jl.IsReconciled)
            .ToListAsync(ct);

        var usedJournalLineIds = new HashSet<int>();

        foreach (var line in unmatchedLines)
        {
            JournalEntryLine? match = null;

            // For positive amounts, match with debit journal lines; for negative amounts, match with credit journal lines
            if (line.Amount > 0)
            {
                match = journalLines
                    .Where(jl => !usedJournalLineIds.Contains(jl.Id))
                    .Where(jl => jl.Debit == line.Amount && jl.Credit == 0)
                    .Where(jl =>
                        (!string.IsNullOrWhiteSpace(line.Reference) && !string.IsNullOrWhiteSpace(jl.JournalEntry.Reference)
                            && jl.JournalEntry.Reference.Contains(line.Reference, StringComparison.OrdinalIgnoreCase))
                        || Math.Abs((jl.JournalEntry.Date - line.Date).TotalDays) <= 3)
                    .OrderBy(jl => Math.Abs((jl.JournalEntry.Date - line.Date).TotalDays))
                    .FirstOrDefault();
            }
            else if (line.Amount < 0)
            {
                var absAmount = Math.Abs(line.Amount);
                match = journalLines
                    .Where(jl => !usedJournalLineIds.Contains(jl.Id))
                    .Where(jl => jl.Credit == absAmount && jl.Debit == 0)
                    .Where(jl =>
                        (!string.IsNullOrWhiteSpace(line.Reference) && !string.IsNullOrWhiteSpace(jl.JournalEntry.Reference)
                            && jl.JournalEntry.Reference.Contains(line.Reference, StringComparison.OrdinalIgnoreCase))
                        || Math.Abs((jl.JournalEntry.Date - line.Date).TotalDays) <= 3)
                    .OrderBy(jl => Math.Abs((jl.JournalEntry.Date - line.Date).TotalDays))
                    .FirstOrDefault();
            }

            if (match != null)
            {
                line.JournalEntryLineId = match.Id;
                line.IsMatched = true;
                usedJournalLineIds.Add(match.Id);
                _lineRepository.Update(line);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
