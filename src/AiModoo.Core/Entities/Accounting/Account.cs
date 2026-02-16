using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class Account : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public AccountTypeEnum AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool IsReconcilable { get; set; }
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}
