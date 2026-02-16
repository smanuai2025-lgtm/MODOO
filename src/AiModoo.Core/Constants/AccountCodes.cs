namespace AiModoo.Core.Constants;

public static class AccountCodes
{
    // Assets: 1xxx
    public const string Cash = "1001";
    public const string Bank = "1002";
    public const string AccountsReceivable = "1100";
    public const string Inventory = "1200";
    public const string StockInput = "1201";
    public const string StockOutput = "1202";
    public const string PrepaidExpenses = "1300";
    public const string FixedAssets = "1500";
    public const string AccumulatedDepreciation = "1501";

    // Liabilities: 2xxx
    public const string AccountsPayable = "2100";
    public const string SalesTaxPayable = "2200";
    public const string PurchaseTaxReceivable = "2201";
    public const string SalariesPayable = "2300";
    public const string SocialInsurancePayable = "2301";
    public const string AccruedExpenses = "2400";

    // Equity: 3xxx
    public const string Capital = "3001";
    public const string RetainedEarnings = "3002";
    public const string CurrentYearEarnings = "3003";

    // Revenue: 4xxx
    public const string SalesRevenue = "4001";
    public const string ServiceRevenue = "4002";
    public const string OtherIncome = "4900";

    // Expenses: 5xxx
    public const string CostOfGoodsSold = "5001";
    public const string SalaryExpense = "5100";
    public const string HousingAllowanceExpense = "5101";
    public const string TransportAllowanceExpense = "5102";
    public const string RentExpense = "5200";
    public const string UtilitiesExpense = "5300";
    public const string BankCharges = "5400";
    public const string DepreciationExpense = "5500";
    public const string InventoryAdjustmentExpense = "5600";
    public const string OtherExpenses = "5900";
}
