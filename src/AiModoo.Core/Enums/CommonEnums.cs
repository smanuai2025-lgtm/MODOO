namespace AiModoo.Core.Enums;

public enum PartnerType
{
    Customer = 1,
    Vendor = 2,
    Employee = 3,
    None = 0
}

public enum ProductType
{
    Storable = 1,
    Consumable = 2,
    Service = 3
}

public enum TrackingType
{
    None = 0,
    Lot = 1,
    Serial = 2
}

public enum LocationType
{
    Internal = 1,
    Customer = 2,
    Vendor = 3,
    InventoryLoss = 4,
    Production = 5,
    Transit = 6
}

public enum PickingTypeCode
{
    Incoming = 1,
    Outgoing = 2,
    Internal = 3
}

public enum RuleAction
{
    Pull = 1,
    Push = 2
}

public enum TaxType
{
    Percentage = 1,
    Fixed = 2
}

public enum PriceComputeType
{
    Fixed = 1,
    Percentage = 2,
    Formula = 3
}

public enum QuotationStatus
{
    Draft = 1,
    Sent = 2,
    Confirmed = 3,
    Cancelled = 4
}

public enum JournalEntryStatus
{
    Draft = 1,
    Posted = 2,
    Cancelled = 3
}

public enum ReconciliationStatus
{
    InProgress = 1,
    Validated = 2
}

public enum EmployeeStatus
{
    Active = 1,
    OnLeave = 2,
    Terminated = 3
}

public enum Gender
{
    Male = 1,
    Female = 2
}

public enum SalaryRuleCategory
{
    Basic = 1,
    Allowance = 2,
    Deduction = 3,
    Gross = 4,
    Net = 5
}

public enum SalaryComputationType
{
    Fixed = 1,
    Percentage = 2,
    Code = 3
}

public enum ApplicantStage
{
    New = 1,
    InitialScreen = 2,
    Interview = 3,
    Offer = 4,
    Hired = 5,
    Refused = 6
}

public enum AppraisalStatus
{
    Draft = 1,
    Confirmed = 2,
    Done = 3
}

public enum AttendanceSource
{
    Manual = 1,
    Biometric = 2,
    Web = 3
}

public enum UomType
{
    Reference = 1,
    Bigger = 2,
    Smaller = 3
}

public enum InvoiceType
{
    CustomerInvoice = 1,
    VendorBill = 2,
    CustomerCreditNote = 3,
    VendorDebitNote = 4
}

public enum TaxGroupType
{
    Group = 1
}

public enum StockMoveStatus
{
    Draft = 1,
    Waiting = 2,
    Ready = 3,
    Done = 4,
    Cancelled = 5
}

public enum StockPickingStatus
{
    Draft = 1,
    Waiting = 2,
    Ready = 3,
    Done = 4,
    Cancelled = 5
}

public enum InventoryAdjustmentStatus
{
    Draft = 1,
    InProgress = 2,
    Validated = 3,
    Cancelled = 4
}

public enum CostMethod
{
    Standard = 1,
    AverageCost = 2,
    FIFO = 3
}

public enum SalesOrderStatus
{
    Quotation = 1,
    QuotationSent = 2,
    SalesOrder = 3,
    Done = 4,
    Cancelled = 5
}

public enum InvoicingPolicy
{
    OrderedQuantities = 1,
    DeliveredQuantities = 2
}

public enum PriceComputeMethod
{
    FixedPrice = 1,
    Discount = 2,
    Formula = 3
}

public enum PurchaseOrderStatus
{
    RFQ = 1,
    RFQSent = 2,
    PurchaseOrder = 3,
    Done = 4,
    Cancelled = 5
}

public enum PosSessionStatus
{
    Opening = 1,
    Opened = 2,
    Closing = 3,
    Closed = 4
}

public enum PosOrderStatus
{
    Draft = 1,
    Paid = 2,
    Done = 3,
    Invoiced = 4,
    Cancelled = 5
}
