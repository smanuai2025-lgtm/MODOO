using AiModoo.Core.Enums;

namespace AiModoo.Core.DTOs.Sales;

public class PartnerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public PartnerType PartnerType { get; set; }
    public bool IsCompany { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int OrderCount { get; set; }
}

public class SalesOrderDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public SalesOrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public bool HasDelivery { get; set; }
    public bool HasInvoice { get; set; }
}

public class SalesDashboardDto
{
    public int TotalOrders { get; set; }
    public int QuotationCount { get; set; }
    public int ConfirmedCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CustomerCount { get; set; }
    public IEnumerable<SalesOrderDto> RecentOrders { get; set; } = Enumerable.Empty<SalesOrderDto>();
}
