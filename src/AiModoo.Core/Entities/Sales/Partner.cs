using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Purchases;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Entities.Sales;

public class Partner : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public PartnerType PartnerType { get; set; } = PartnerType.Customer;
    public bool IsCompany { get; set; } = true;

    // Contact
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }

    // Address
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    // Tax & ID
    public string? TaxNumber { get; set; }
    public string? CommercialRegister { get; set; }

    // Accounting
    public int? AccountReceivableId { get; set; }
    public Account? AccountReceivable { get; set; }
    public int? AccountPayableId { get; set; }
    public Account? AccountPayable { get; set; }

    // Payment
    public int? PaymentTermId { get; set; }
    public PaymentTerm? PaymentTerm { get; set; }
    public decimal CreditLimit { get; set; }

    // Sales
    public int? PricelistId { get; set; }
    public Pricelist? Pricelist { get; set; }
    public int? SalesTeamId { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
