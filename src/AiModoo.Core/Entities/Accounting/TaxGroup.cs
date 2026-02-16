using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

/// <summary>
/// A composite tax that contains multiple child taxes
/// Example: Group tax = VAT 15% + Municipal tax 2%
/// </summary>
public class TaxGroup : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TaxGroupDetail> Details { get; set; } = new List<TaxGroupDetail>();

    /// <summary>
    /// Calculates the total effective rate of all child taxes
    /// </summary>
    public decimal TotalRate => Details.Sum(d => d.Tax?.Rate ?? 0);
}

public class TaxGroupDetail : BaseEntity
{
    public int TaxGroupId { get; set; }
    public TaxGroup TaxGroup { get; set; } = null!;
    public int TaxId { get; set; }
    public Tax Tax { get; set; } = null!;
    public int Sequence { get; set; }
}
