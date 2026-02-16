namespace AiModoo.Core.Exceptions;

public class InsufficientStockException : BusinessRuleException
{
    public int ProductId { get; }
    public decimal RequestedQuantity { get; }
    public decimal AvailableQuantity { get; }

    public InsufficientStockException(int productId, decimal requested, decimal available)
        : base("INSUFFICIENT_STOCK", $"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableQuantity = available;
    }
}
