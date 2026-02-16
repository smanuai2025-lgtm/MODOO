using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Entities.Inventory;

public class StockLot : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public DateTime? ExpirationDate { get; set; }
    public string? Notes { get; set; }

    public ICollection<StockQuant> StockQuants { get; set; } = new List<StockQuant>();
    public ICollection<StockMove> StockMoves { get; set; } = new List<StockMove>();
}
