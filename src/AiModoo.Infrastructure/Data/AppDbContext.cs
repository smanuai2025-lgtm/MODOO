using AiModoo.Core.Entities.Common;
using AiModoo.Core.Entities.Identity;
using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.POS;
using AiModoo.Core.Entities.Purchases;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Interfaces.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    // Accounting
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Journal> Journals => Set<Journal>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<TaxGroup> TaxGroups => Set<TaxGroup>();
    public DbSet<TaxGroupDetail> TaxGroupDetails => Set<TaxGroupDetail>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
    public DbSet<BankReconciliationLine> BankReconciliationLines => Set<BankReconciliationLine>();

    // Inventory
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<UnitOfMeasureCategory> UnitOfMeasureCategories => Set<UnitOfMeasureCategory>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StockPickingType> StockPickingTypes => Set<StockPickingType>();
    public DbSet<StockPicking> StockPickings => Set<StockPicking>();
    public DbSet<StockMove> StockMoves => Set<StockMove>();
    public DbSet<StockQuant> StockQuants => Set<StockQuant>();
    public DbSet<StockLot> StockLots => Set<StockLot>();
    public DbSet<InventoryAdjustment> InventoryAdjustments => Set<InventoryAdjustment>();
    public DbSet<InventoryAdjustmentLine> InventoryAdjustmentLines => Set<InventoryAdjustmentLine>();
    public DbSet<ReorderingRule> ReorderingRules => Set<ReorderingRule>();

    // Sales
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PaymentTerm> PaymentTerms => Set<PaymentTerm>();
    public DbSet<PaymentTermLine> PaymentTermLines => Set<PaymentTermLine>();
    public DbSet<Pricelist> Pricelists => Set<Pricelist>();
    public DbSet<PricelistItem> PricelistItems => Set<PricelistItem>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<SalesTeam> SalesTeams => Set<SalesTeam>();

    // Purchases
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    // POS
    public DbSet<PosConfig> PosConfigs => Set<PosConfig>();
    public DbSet<PosSession> PosSessions => Set<PosSession>();
    public DbSet<PosOrder> PosOrders => Set<PosOrder>();
    public DbSet<PosOrderLine> PosOrderLines => Set<PosOrderLine>();
    public DbSet<PosPayment> PosPayments => Set<PosPayment>();

    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTimeService? _dateTimeService;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { builder });
            }
        }
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : AuditableEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeService?.UtcNow ?? DateTime.UtcNow;
        var userId = _currentUserService?.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedById = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedById = userId;
                    break;
                case EntityState.Deleted:
                    // Convert to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
