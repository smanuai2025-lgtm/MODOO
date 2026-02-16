using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AiModoo.Services.Accounting;

public class BankStatementService : IBankStatementService
{
    private readonly IRepository<BankStatement> _repository;
    private readonly IRepository<BankStatementLine> _lineRepository;
    private readonly IRepository<JournalEntryLine> _journalLineRepository;
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BankStatementService(
        IRepository<BankStatement> repository,
        IRepository<BankStatementLine> lineRepository,
        IRepository<JournalEntryLine> journalLineRepository,
        IRepository<BankAccount> bankAccountRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _journalLineRepository = journalLineRepository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<BankStatement>> GetAllAsync(CancellationToken ct = default)
        => await _repository.Query()
            .Include(s => s.BankAccount)
            .Include(s => s.Lines)
            .OrderByDescending(s => s.Date)
            .ToListAsync(ct);

    public async Task<BankStatement?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repository.Query()
            .Include(s => s.BankAccount)
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<BankStatement> CreateAsync(BankStatement statement, CancellationToken ct = default)
    {
        _ = await _bankAccountRepository.GetByIdAsync(statement.BankAccountId, ct)
            ?? throw new NotFoundException(nameof(BankAccount), statement.BankAccountId);

        await _repository.AddAsync(statement, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return statement;
    }

    public async Task<BankStatement> ImportCsvAsync(int bankAccountId, Stream csvStream, CancellationToken ct = default)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, ct)
            ?? throw new NotFoundException(nameof(BankAccount), bankAccountId);

        using var reader = new StreamReader(csvStream);
        var headerLine = await reader.ReadLineAsync(ct)
            ?? throw new BusinessRuleException("EMPTY_CSV", "CSV file is empty.");

        var headers = headerLine.Split(',')
            .Select(h => h.Trim().ToLowerInvariant())
            .ToArray();

        bool hasDebitCredit = headers.Contains("debit") && headers.Contains("credit");
        bool hasAmount = headers.Contains("amount");

        if (!hasDebitCredit && !hasAmount)
            throw new BusinessRuleException("INVALID_CSV_FORMAT", "CSV must contain either 'Amount' or 'Debit' and 'Credit' columns.");

        int dateIdx = Array.IndexOf(headers, "date");
        int refIdx = Array.IndexOf(headers, "reference");
        int descIdx = Array.IndexOf(headers, "description");
        int amountIdx = hasAmount ? Array.IndexOf(headers, "amount") : -1;
        int debitIdx = hasDebitCredit ? Array.IndexOf(headers, "debit") : -1;
        int creditIdx = hasDebitCredit ? Array.IndexOf(headers, "credit") : -1;

        if (dateIdx < 0)
            throw new BusinessRuleException("INVALID_CSV_FORMAT", "CSV must contain a 'Date' column.");

        var lines = new List<BankStatementLine>();
        string? line;
        int lineNumber = 1;

        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = line.Split(',');

            if (!DateTime.TryParse(values[dateIdx].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new BusinessRuleException("INVALID_CSV_DATA", $"Invalid date at line {lineNumber}.");

            decimal amount;
            if (hasAmount)
            {
                if (!decimal.TryParse(values[amountIdx].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                    throw new BusinessRuleException("INVALID_CSV_DATA", $"Invalid amount at line {lineNumber}.");
            }
            else
            {
                decimal.TryParse(values[debitIdx].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var debit);
                decimal.TryParse(values[creditIdx].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var credit);
                amount = debit - credit;
            }

            var reference = refIdx >= 0 && refIdx < values.Length ? values[refIdx].Trim() : null;
            var description = descIdx >= 0 && descIdx < values.Length ? values[descIdx].Trim() : null;

            lines.Add(new BankStatementLine
            {
                Date = date,
                Reference = reference,
                Description = description,
                Amount = amount
            });
        }

        if (lines.Count == 0)
            throw new BusinessRuleException("EMPTY_CSV", "CSV file contains no data lines.");

        var minDate = lines.Min(l => l.Date);
        var maxDate = lines.Max(l => l.Date);

        var statement = new BankStatement
        {
            BankAccountId = bankAccountId,
            Reference = $"Import-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            Date = DateTime.UtcNow,
            PeriodStart = minDate,
            PeriodEnd = maxDate,
            OpeningBalance = bankAccount.Balance,
            ClosingBalance = bankAccount.Balance + lines.Sum(l => l.Amount),
            IsProcessed = false,
            Lines = lines
        };

        await _repository.AddAsync(statement, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return statement;
    }

    public async Task ProcessAsync(int statementId, CancellationToken ct = default)
    {
        var statement = await GetByIdAsync(statementId, ct)
            ?? throw new NotFoundException(nameof(BankStatement), statementId);

        if (statement.IsProcessed)
            throw new BusinessRuleException("ALREADY_PROCESSED", "This bank statement has already been processed.");

        statement.IsProcessed = true;
        _repository.Update(statement);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<BankStatementLine>> AutoMatchAsync(int statementId, CancellationToken ct = default)
    {
        var statement = await _repository.Query()
            .Include(s => s.BankAccount)
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == statementId, ct)
            ?? throw new NotFoundException(nameof(BankStatement), statementId);

        var unreconciledLines = statement.Lines
            .Where(l => !l.IsReconciled && !l.MatchedJournalEntryLineId.HasValue)
            .ToList();

        if (unreconciledLines.Count == 0)
            return Array.Empty<BankStatementLine>();

        // Get unreconciled journal entry lines for the same bank account
        var journalLines = await _journalLineRepository.Query()
            .Include(jl => jl.JournalEntry)
            .Include(jl => jl.Account)
            .Where(jl => jl.Account.Id == statement.BankAccount.AccountId
                && jl.JournalEntry.Status == JournalEntryStatus.Posted
                && !jl.IsReconciled)
            .ToListAsync(ct);

        var matchedLines = new List<BankStatementLine>();
        var usedJournalLineIds = new HashSet<int>();

        foreach (var statementLine in unreconciledLines)
        {
            var statementAmount = statementLine.Amount;

            // Try to find a matching journal entry line
            JournalEntryLine? match = null;

            // Strategy 1: Exact amount match + date within 5 days
            match = journalLines
                .Where(jl => !usedJournalLineIds.Contains(jl.Id))
                .Where(jl =>
                {
                    var journalAmount = jl.Debit - jl.Credit;
                    return journalAmount == statementAmount;
                })
                .Where(jl => Math.Abs((jl.JournalEntry.Date - statementLine.Date).TotalDays) <= 5)
                .OrderBy(jl => Math.Abs((jl.JournalEntry.Date - statementLine.Date).TotalDays))
                .FirstOrDefault();

            // Strategy 2: Reference match
            if (match == null && !string.IsNullOrWhiteSpace(statementLine.Reference))
            {
                match = journalLines
                    .Where(jl => !usedJournalLineIds.Contains(jl.Id))
                    .Where(jl =>
                    {
                        var journalAmount = jl.Debit - jl.Credit;
                        return journalAmount == statementAmount;
                    })
                    .Where(jl => !string.IsNullOrWhiteSpace(jl.JournalEntry.Reference)
                        && jl.JournalEntry.Reference.Contains(statementLine.Reference, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
            }

            if (match != null)
            {
                statementLine.MatchedJournalEntryLineId = match.Id;
                usedJournalLineIds.Add(match.Id);
                _lineRepository.Update(statementLine);
                matchedLines.Add(statementLine);
            }
        }

        if (matchedLines.Count > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        return matchedLines;
    }
}
