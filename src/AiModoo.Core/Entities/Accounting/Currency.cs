using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int DecimalPlaces { get; set; } = 2;
    public ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
}
