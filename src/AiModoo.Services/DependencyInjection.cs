using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.POS;
using AiModoo.Core.Interfaces.Purchases;
using AiModoo.Core.Interfaces.Sales;
using AiModoo.Services.Accounting;
using AiModoo.Services.Inventory;
using AiModoo.Services.Mapping;
using AiModoo.Services.POS;
using AiModoo.Services.Purchases;
using AiModoo.Services.Reports;
using AiModoo.Services.Sales;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AiModoo.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Accounting Services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IJournalEntryService, JournalEntryService>();
        services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IFinancialReportService, FinancialReportService>();
        services.AddScoped<IBankReconciliationService, BankReconciliationService>();
        services.AddScoped<IBankStatementService, BankStatementService>();
        services.AddScoped<IAccountingIntegrationService, AccountingIntegrationService>();

        // Inventory Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IStockPickingService, StockPickingService>();
        services.AddScoped<IInventoryAdjustmentService, InventoryAdjustmentService>();
        services.AddScoped<IStockQuantService, StockQuantService>();

        // Sales Services
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IPricelistService, PricelistService>();

        // Purchases Services
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

        // POS Services
        services.AddScoped<IPosService, PosService>();

        // Report Services
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
