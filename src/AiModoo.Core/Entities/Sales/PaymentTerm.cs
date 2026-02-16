using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Sales;

public class PaymentTerm : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public ICollection<PaymentTermLine> Lines { get; set; } = new List<PaymentTermLine>();
}

public class PaymentTermLine : BaseEntity
{
    public int PaymentTermId { get; set; }
    public PaymentTerm PaymentTerm { get; set; } = null!;
    public int Sequence { get; set; }
    public decimal Percentage { get; set; } = 100;
    public int Days { get; set; }
    public int DayOfMonth { get; set; }
}
