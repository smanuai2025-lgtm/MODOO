using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class Journal : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string Code { get; set; } = string.Empty;
    public JournalTypeEnum Type { get; set; }
    public int? DefaultDebitAccountId { get; set; }
    public Account? DefaultDebitAccount { get; set; }
    public int? DefaultCreditAccountId { get; set; }
    public Account? DefaultCreditAccount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SequencePrefix { get; set; }
    public int NextSequenceNumber { get; set; } = 1;
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}
