using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.POS;

public class PosOrder : AuditableEntity
{
    public string Number { get; set; } = string.Empty;
    public PosOrderStatus Status { get; set; } = PosOrderStatus.Draft;

    public int SessionId { get; set; }
    public PosSession Session { get; set; } = null!;

    public int? PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public string? PartnerName { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }

    public string? Notes { get; set; }
    public bool IsReturn { get; set; }

    public ICollection<PosOrderLine> Lines { get; set; } = new List<PosOrderLine>();
    public ICollection<PosPayment> Payments { get; set; } = new List<PosPayment>();
}
