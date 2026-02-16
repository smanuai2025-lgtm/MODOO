using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class FiscalPeriod : BaseEntity
{
    public int FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
}
