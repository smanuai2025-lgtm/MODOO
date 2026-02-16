using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Accounting;

public class ExchangeRate : BaseEntity
{
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    public decimal Rate { get; set; }
    public DateTime Date { get; set; }
}
