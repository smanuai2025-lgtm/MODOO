using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.POS;

public class PosPayment : BaseEntity
{
    public int OrderId { get; set; }
    public PosOrder Order { get; set; } = null!;

    public int PaymentMethodId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;

    public decimal Amount { get; set; }
}
