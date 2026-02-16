using AiModoo.Core.Constants;
using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AccountType = AiModoo.Core.Enums.AccountTypeEnum;
using JournalType = AiModoo.Core.Enums.JournalTypeEnum;

namespace AiModoo.Infrastructure.Data.Seeds;

public static class AccountingSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedCurrenciesAsync(context);
        await SeedAccountsAsync(context);
        await SeedJournalsAsync(context);
        await SeedPaymentMethodsAsync(context);
        await SeedFiscalYearAsync(context);
        await SeedTaxesAsync(context);
    }

    private static async Task SeedCurrenciesAsync(AppDbContext context)
    {
        if (await context.Currencies.AnyAsync()) return;

        var currencies = new List<Currency>
        {
            new() { Code = "SAR", Name = "Saudi Riyal", NameAr = "ريال سعودي", Symbol = "﷼", IsDefault = true, IsActive = true, DecimalPlaces = 2 },
            new() { Code = "USD", Name = "US Dollar", NameAr = "دولار أمريكي", Symbol = "$", IsDefault = false, IsActive = true, DecimalPlaces = 2 },
            new() { Code = "EUR", Name = "Euro", NameAr = "يورو", Symbol = "€", IsDefault = false, IsActive = true, DecimalPlaces = 2 },
            new() { Code = "AED", Name = "UAE Dirham", NameAr = "درهم إماراتي", Symbol = "د.إ", IsDefault = false, IsActive = true, DecimalPlaces = 2 },
            new() { Code = "EGP", Name = "Egyptian Pound", NameAr = "جنيه مصري", Symbol = "ج.م", IsDefault = false, IsActive = true, DecimalPlaces = 2 },
        };

        await context.Currencies.AddRangeAsync(currencies);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAccountsAsync(AppDbContext context)
    {
        if (await context.Accounts.AnyAsync()) return;

        var accounts = new List<Account>
        {
            // Assets (1xxx)
            new() { Code = "1000", Name = "Assets", NameAr = "الأصول", AccountType = AccountType.Asset, Level = 0, IsActive = true },
            new() { Code = AccountCodes.Cash, Name = "Cash", NameAr = "النقدية", AccountType = AccountType.Asset, Level = 1, IsActive = true },
            new() { Code = AccountCodes.Bank, Name = "Bank Account", NameAr = "الحساب البنكي", AccountType = AccountType.Asset, Level = 1, IsActive = true, IsReconcilable = true },
            new() { Code = AccountCodes.AccountsReceivable, Name = "Accounts Receivable", NameAr = "الذمم المدينة", AccountType = AccountType.Asset, Level = 1, IsActive = true, IsReconcilable = true },
            new() { Code = AccountCodes.Inventory, Name = "Inventory", NameAr = "المخزون", AccountType = AccountType.Asset, Level = 1, IsActive = true },
            new() { Code = AccountCodes.PrepaidExpenses, Name = "Prepaid Expenses", NameAr = "مصروفات مدفوعة مقدماً", AccountType = AccountType.Asset, Level = 1, IsActive = true },
            new() { Code = "1600", Name = "Fixed Assets", NameAr = "الأصول الثابتة", AccountType = AccountType.Asset, Level = 1, IsActive = true },
            new() { Code = "1700", Name = "Accumulated Depreciation", NameAr = "مجمع الإهلاك", AccountType = AccountType.Asset, Level = 1, IsActive = true },

            // Liabilities (2xxx)
            new() { Code = "2000", Name = "Liabilities", NameAr = "الالتزامات", AccountType = AccountType.Liability, Level = 0, IsActive = true },
            new() { Code = AccountCodes.AccountsPayable, Name = "Accounts Payable", NameAr = "الذمم الدائنة", AccountType = AccountType.Liability, Level = 1, IsActive = true, IsReconcilable = true },
            new() { Code = AccountCodes.SalesTaxPayable, Name = "Tax Payable", NameAr = "ضريبة مستحقة", AccountType = AccountType.Liability, Level = 1, IsActive = true },
            new() { Code = AccountCodes.SalariesPayable, Name = "Salaries Payable", NameAr = "رواتب مستحقة", AccountType = AccountType.Liability, Level = 1, IsActive = true },
            new() { Code = "2400", Name = "Unearned Revenue", NameAr = "إيرادات غير مكتسبة", AccountType = AccountType.Liability, Level = 1, IsActive = true },

            // Equity (3xxx)
            new() { Code = "3000", Name = "Equity", NameAr = "حقوق الملكية", AccountType = AccountType.Equity, Level = 0, IsActive = true },
            new() { Code = AccountCodes.Capital, Name = "Capital", NameAr = "رأس المال", AccountType = AccountType.Equity, Level = 1, IsActive = true },
            new() { Code = AccountCodes.RetainedEarnings, Name = "Retained Earnings", NameAr = "الأرباح المحتجزة", AccountType = AccountType.Equity, Level = 1, IsActive = true },

            // Revenue (4xxx)
            new() { Code = "4000", Name = "Revenue", NameAr = "الإيرادات", AccountType = AccountType.Revenue, Level = 0, IsActive = true },
            new() { Code = AccountCodes.SalesRevenue, Name = "Sales Revenue", NameAr = "إيرادات المبيعات", AccountType = AccountType.Revenue, Level = 1, IsActive = true },
            new() { Code = AccountCodes.ServiceRevenue, Name = "Service Revenue", NameAr = "إيرادات الخدمات", AccountType = AccountType.Revenue, Level = 1, IsActive = true },
            new() { Code = AccountCodes.OtherIncome, Name = "Other Income", NameAr = "إيرادات أخرى", AccountType = AccountType.Revenue, Level = 1, IsActive = true },

            // Expenses (5xxx)
            new() { Code = "5000", Name = "Expenses", NameAr = "المصروفات", AccountType = AccountType.Expense, Level = 0, IsActive = true },
            new() { Code = AccountCodes.CostOfGoodsSold, Name = "Cost of Goods Sold", NameAr = "تكلفة البضاعة المباعة", AccountType = AccountType.Expense, Level = 1, IsActive = true },
            new() { Code = AccountCodes.SalaryExpense, Name = "Salary Expense", NameAr = "مصروف الرواتب", AccountType = AccountType.Expense, Level = 1, IsActive = true },
            new() { Code = AccountCodes.RentExpense, Name = "Rent Expense", NameAr = "مصروف الإيجار", AccountType = AccountType.Expense, Level = 1, IsActive = true },
            new() { Code = AccountCodes.UtilitiesExpense, Name = "Utilities Expense", NameAr = "مصروف المرافق", AccountType = AccountType.Expense, Level = 1, IsActive = true },
            new() { Code = AccountCodes.DepreciationExpense, Name = "Depreciation Expense", NameAr = "مصروف الإهلاك", AccountType = AccountType.Expense, Level = 1, IsActive = true },
            new() { Code = AccountCodes.OtherExpenses, Name = "Other Expenses", NameAr = "مصروفات أخرى", AccountType = AccountType.Expense, Level = 1, IsActive = true },
        };

        // Set parent accounts
        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();

        // Set parent relationships
        var allAccounts = await context.Accounts.ToListAsync();
        foreach (var acc in allAccounts.Where(a => a.Level == 1))
        {
            var parentCode = acc.Code[0] + "000";
            var parent = allAccounts.FirstOrDefault(a => a.Code == parentCode);
            if (parent != null)
            {
                acc.ParentAccountId = parent.Id;
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedJournalsAsync(AppDbContext context)
    {
        if (await context.Journals.AnyAsync()) return;

        var cashAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodes.Cash);
        var bankAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodes.Bank);
        var receivableAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodes.AccountsReceivable);
        var payableAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodes.AccountsPayable);
        var salesAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodes.SalesRevenue);

        var journals = new List<Journal>
        {
            new() { Code = "SAL", Name = "Sales Journal", NameAr = "يومية المبيعات", Type = JournalType.Sale, SequencePrefix = "INV", NextSequenceNumber = 1, IsActive = true,
                DefaultDebitAccountId = receivableAccount?.Id, DefaultCreditAccountId = salesAccount?.Id },
            new() { Code = "PUR", Name = "Purchase Journal", NameAr = "يومية المشتريات", Type = JournalType.Purchase, SequencePrefix = "BILL", NextSequenceNumber = 1, IsActive = true,
                DefaultDebitAccountId = null, DefaultCreditAccountId = payableAccount?.Id },
            new() { Code = "CSH", Name = "Cash Journal", NameAr = "يومية النقدية", Type = JournalType.Cash, SequencePrefix = "CSH", NextSequenceNumber = 1, IsActive = true,
                DefaultDebitAccountId = cashAccount?.Id, DefaultCreditAccountId = cashAccount?.Id },
            new() { Code = "BNK", Name = "Bank Journal", NameAr = "يومية البنك", Type = JournalType.Bank, SequencePrefix = "BNK", NextSequenceNumber = 1, IsActive = true,
                DefaultDebitAccountId = bankAccount?.Id, DefaultCreditAccountId = bankAccount?.Id },
            new() { Code = "GEN", Name = "General Journal", NameAr = "اليومية العامة", Type = JournalType.General, SequencePrefix = "JE", NextSequenceNumber = 1, IsActive = true },
        };

        await context.Journals.AddRangeAsync(journals);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPaymentMethodsAsync(AppDbContext context)
    {
        if (await context.PaymentMethods.AnyAsync()) return;

        var methods = new List<PaymentMethod>
        {
            new() { Name = "Cash", NameAr = "نقدي", IsActive = true },
            new() { Name = "Bank Transfer", NameAr = "تحويل بنكي", IsActive = true },
            new() { Name = "Check", NameAr = "شيك", IsActive = true },
            new() { Name = "Credit Card", NameAr = "بطاقة ائتمان", IsActive = true },
            new() { Name = "Mada", NameAr = "مدى", IsActive = true },
        };

        await context.PaymentMethods.AddRangeAsync(methods);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFiscalYearAsync(AppDbContext context)
    {
        if (await context.FiscalYears.AnyAsync()) return;

        var year = DateTime.Now.Year;
        var fiscalYear = new FiscalYear
        {
            Name = $"FY {year}",
            StartDate = new DateTime(year, 1, 1),
            EndDate = new DateTime(year, 12, 31),
            IsClosed = false,
            Periods = new List<FiscalPeriod>()
        };

        for (int m = 1; m <= 12; m++)
        {
            fiscalYear.Periods.Add(new FiscalPeriod
            {
                Name = $"{year}/{m:D2}",
                StartDate = new DateTime(year, m, 1),
                EndDate = new DateTime(year, m, DateTime.DaysInMonth(year, m)),
                IsClosed = false
            });
        }

        await context.FiscalYears.AddAsync(fiscalYear);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTaxesAsync(AppDbContext context)
    {
        if (await context.Taxes.AnyAsync()) return;

        var taxPayableAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodes.SalesTaxPayable);

        var taxes = new List<Tax>
        {
            new() { Name = "VAT 15%", NameAr = "ضريبة القيمة المضافة 15%", Rate = 15m, Type = TaxType.Percentage, IsActive = true, IncludedInPrice = false,
                SalesAccountId = taxPayableAccount?.Id, PurchaseAccountId = taxPayableAccount?.Id },
            new() { Name = "VAT 0%", NameAr = "ضريبة القيمة المضافة 0%", Rate = 0m, Type = TaxType.Percentage, IsActive = true, IncludedInPrice = false,
                SalesAccountId = taxPayableAccount?.Id, PurchaseAccountId = taxPayableAccount?.Id },
            new() { Name = "Exempt", NameAr = "معفى", Rate = 0m, Type = TaxType.Percentage, IsActive = true, IncludedInPrice = false,
                SalesAccountId = taxPayableAccount?.Id, PurchaseAccountId = taxPayableAccount?.Id },
        };

        await context.Taxes.AddRangeAsync(taxes);
        await context.SaveChangesAsync();
    }
}
