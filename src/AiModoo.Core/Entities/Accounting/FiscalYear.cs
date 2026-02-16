using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class FiscalYear : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public ICollection<FiscalPeriod> Periods { get; set; } = new List<FiscalPeriod>();
}
