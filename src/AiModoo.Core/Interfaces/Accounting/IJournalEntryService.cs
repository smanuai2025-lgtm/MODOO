using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IJournalEntryService
{
    Task<IReadOnlyList<JournalEntry>> GetAllAsync(CancellationToken ct = default);
    Task<JournalEntry?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<JournalEntry> CreateAsync(JournalEntry entry, CancellationToken ct = default);
    Task PostAsync(int id, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
    Task<JournalEntry> ReverseAsync(int id, DateTime reversalDate, CancellationToken ct = default);
    Task<string> GenerateNumberAsync(int journalId, CancellationToken ct = default);
}
