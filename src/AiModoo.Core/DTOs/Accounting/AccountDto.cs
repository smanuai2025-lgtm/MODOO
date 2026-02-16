using AiModoo.Core.Enums;

namespace AiModoo.Core.DTOs.Accounting;

public class AccountDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public AccountTypeEnum AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public decimal Balance { get; set; }
    public ICollection<AccountDto>? ChildAccounts { get; set; }
}
