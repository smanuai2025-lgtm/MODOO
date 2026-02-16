namespace AiModoo.Core.Enums;

public enum PaymentStatus
{
    Draft = 1,
    Posted = 2,
    Reconciled = 3,
    Cancelled = 4
}

public enum PaymentType
{
    Inbound = 1,   // Customer payment
    Outbound = 2,  // Vendor payment
    Transfer = 3   // Internal transfer
}
