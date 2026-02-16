namespace AiModoo.Core.Enums;

public enum OrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Done = 3,
    Cancelled = 4
}

public enum DeliveryStatus
{
    Nothing = 0,
    Pending = 1,
    Partial = 2,
    Full = 3
}

public enum InvoicingStatus
{
    Nothing = 0,
    ToInvoice = 1,
    FullyInvoiced = 2
}
