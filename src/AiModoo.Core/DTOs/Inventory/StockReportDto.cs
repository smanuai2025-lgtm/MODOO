namespace AiModoo.Core.DTOs.Inventory;

public class StockReportDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductNameAr { get; set; }
    public string? InternalReference { get; set; }
    public string? CategoryName { get; set; }
    public string? UomName { get; set; }
    public decimal OnHand { get; set; }
    public decimal Reserved { get; set; }
    public decimal Available { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalValue { get; set; }
    public string? WarehouseName { get; set; }
    public string? LocationName { get; set; }
}

public class StockMoveReportDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? UomName { get; set; }
    public string SourceLocation { get; set; } = string.Empty;
    public string DestLocation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
}
