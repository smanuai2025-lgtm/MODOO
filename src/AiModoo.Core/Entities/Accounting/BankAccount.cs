using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class BankAccount : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public int? JournalId { get; set; }
    public Journal? Journal { get; set; }
}
