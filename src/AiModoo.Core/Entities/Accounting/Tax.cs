using AiModoo.Core.Entities.Common;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Accounting;

public class Tax : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public decimal Rate { get; set; }
    public TaxType Type { get; set; } = TaxType.Percentage;
    public int? SalesAccountId { get; set; }
    public Account? SalesAccount { get; set; }
    public int? PurchaseAccountId { get; set; }
    public Account? PurchaseAccount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IncludedInPrice { get; set; }
}
