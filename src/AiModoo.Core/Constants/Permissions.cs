namespace AiModoo.Core.Constants;

public static class Permissions
{
    public static class Accounting
    {
        public const string View = "Permissions.Accounting.View";
        public const string Create = "Permissions.Accounting.Create";
        public const string Edit = "Permissions.Accounting.Edit";
        public const string Delete = "Permissions.Accounting.Delete";
        public const string Post = "Permissions.Accounting.Post";
        public const string ClosePeriod = "Permissions.Accounting.ClosePeriod";
    }

    public static class Sales
    {
        public const string View = "Permissions.Sales.View";
        public const string Create = "Permissions.Sales.Create";
        public const string Edit = "Permissions.Sales.Edit";
        public const string Delete = "Permissions.Sales.Delete";
        public const string Confirm = "Permissions.Sales.Confirm";
        public const string Invoice = "Permissions.Sales.Invoice";
    }

    public static class Purchases
    {
        public const string View = "Permissions.Purchases.View";
        public const string Create = "Permissions.Purchases.Create";
        public const string Edit = "Permissions.Purchases.Edit";
        public const string Delete = "Permissions.Purchases.Delete";
        public const string Confirm = "Permissions.Purchases.Confirm";
        public const string Approve = "Permissions.Purchases.Approve";
    }

    public static class Inventory
    {
        public const string View = "Permissions.Inventory.View";
        public const string Create = "Permissions.Inventory.Create";
        public const string Edit = "Permissions.Inventory.Edit";
        public const string Delete = "Permissions.Inventory.Delete";
        public const string Validate = "Permissions.Inventory.Validate";
        public const string Adjust = "Permissions.Inventory.Adjust";
    }

    public static class POS
    {
        public const string View = "Permissions.POS.View";
        public const string Use = "Permissions.POS.Use";
        public const string Configure = "Permissions.POS.Configure";
        public const string CloseSession = "Permissions.POS.CloseSession";
    }

    public static class HR
    {
        public const string View = "Permissions.HR.View";
        public const string Create = "Permissions.HR.Create";
        public const string Edit = "Permissions.HR.Edit";
        public const string Delete = "Permissions.HR.Delete";
        public const string ApproveLeave = "Permissions.HR.ApproveLeave";
        public const string RunPayroll = "Permissions.HR.RunPayroll";
    }
}
