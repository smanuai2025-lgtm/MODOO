using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class PaymentMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;
}
