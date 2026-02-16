using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiModoo.Infrastructure.Data.Seeds;

public static class InventorySeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Products.AnyAsync())
            return;

        // 1. UoM Categories
        var uomCategories = new List<UnitOfMeasureCategory>
        {
            new() { Name = "Unit", NameAr = "وحدة", IsActive = true },
            new() { Name = "Weight", NameAr = "وزن", IsActive = true },
            new() { Name = "Volume", NameAr = "حجم", IsActive = true },
            new() { Name = "Length", NameAr = "طول", IsActive = true },
            new() { Name = "Time", NameAr = "وقت", IsActive = true }
        };
        context.UnitOfMeasureCategories.AddRange(uomCategories);
        await context.SaveChangesAsync();

        // 2. Units of Measure
        var uoms = new List<UnitOfMeasure>
        {
            new() { Name = "Unit(s)", NameAr = "وحدة", CategoryId = uomCategories[0].Id, Type = UomType.Reference, Ratio = 1 },
            new() { Name = "Dozen", NameAr = "درزن", CategoryId = uomCategories[0].Id, Type = UomType.Bigger, Ratio = 12 },
            new() { Name = "Box (10)", NameAr = "صندوق (10)", CategoryId = uomCategories[0].Id, Type = UomType.Bigger, Ratio = 10 },
            new() { Name = "kg", NameAr = "كجم", CategoryId = uomCategories[1].Id, Type = UomType.Reference, Ratio = 1 },
            new() { Name = "g", NameAr = "جرام", CategoryId = uomCategories[1].Id, Type = UomType.Smaller, Ratio = 0.001m },
            new() { Name = "Ton", NameAr = "طن", CategoryId = uomCategories[1].Id, Type = UomType.Bigger, Ratio = 1000 },
            new() { Name = "Liter", NameAr = "لتر", CategoryId = uomCategories[2].Id, Type = UomType.Reference, Ratio = 1 },
            new() { Name = "mL", NameAr = "مل", CategoryId = uomCategories[2].Id, Type = UomType.Smaller, Ratio = 0.001m },
            new() { Name = "Meter", NameAr = "متر", CategoryId = uomCategories[3].Id, Type = UomType.Reference, Ratio = 1 },
            new() { Name = "cm", NameAr = "سم", CategoryId = uomCategories[3].Id, Type = UomType.Smaller, Ratio = 0.01m },
            new() { Name = "Hour", NameAr = "ساعة", CategoryId = uomCategories[4].Id, Type = UomType.Reference, Ratio = 1 },
            new() { Name = "Day", NameAr = "يوم", CategoryId = uomCategories[4].Id, Type = UomType.Bigger, Ratio = 8 }
        };
        context.UnitOfMeasures.AddRange(uoms);
        await context.SaveChangesAsync();

        // 3. Product Categories
        var categories = new List<ProductCategory>
        {
            new() { Name = "Electronics", NameAr = "إلكترونيات", Level = 1, IsActive = true },
            new() { Name = "Office Supplies", NameAr = "مستلزمات مكتبية", Level = 1, IsActive = true },
            new() { Name = "Services", NameAr = "خدمات", Level = 1, IsActive = true },
            new() { Name = "Raw Materials", NameAr = "مواد خام", Level = 1, IsActive = true },
            new() { Name = "Consumables", NameAr = "مستهلكات", Level = 1, IsActive = true }
        };
        context.ProductCategories.AddRange(categories);
        await context.SaveChangesAsync();

        // Sub-categories
        var subCategories = new List<ProductCategory>
        {
            new() { Name = "Computers", NameAr = "أجهزة كمبيوتر", ParentCategoryId = categories[0].Id, Level = 2, IsActive = true },
            new() { Name = "Accessories", NameAr = "إكسسوارات", ParentCategoryId = categories[0].Id, Level = 2, IsActive = true },
            new() { Name = "Paper Products", NameAr = "منتجات ورقية", ParentCategoryId = categories[1].Id, Level = 2, IsActive = true }
        };
        context.ProductCategories.AddRange(subCategories);
        await context.SaveChangesAsync();

        // 4. Warehouse & Locations
        // Virtual locations (no warehouse)
        var vendorLoc = new Location { Name = "Vendors", NameAr = "الموردين", CompleteName = "Virtual/Vendors", LocationType = LocationType.Vendor, IsActive = true };
        var customerLoc = new Location { Name = "Customers", NameAr = "العملاء", CompleteName = "Virtual/Customers", LocationType = LocationType.Customer, IsActive = true };
        var inventoryLossLoc = new Location { Name = "Inventory Loss", NameAr = "خسائر المخزون", CompleteName = "Virtual/Inventory Loss", LocationType = LocationType.InventoryLoss, IsActive = true };
        context.Locations.AddRange(vendorLoc, customerLoc, inventoryLossLoc);
        await context.SaveChangesAsync();

        // Main warehouse
        var warehouse = new Warehouse { Name = "Main Warehouse", NameAr = "المستودع الرئيسي", Code = "WH", IsActive = true };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        var stockLoc = new Location { Name = "WH/Stock", NameAr = "المستودع/المخزن", CompleteName = "Main Warehouse/Stock", LocationType = LocationType.Internal, WarehouseId = warehouse.Id, IsActive = true };
        var inputLoc = new Location { Name = "WH/Input", NameAr = "المستودع/الاستلام", CompleteName = "Main Warehouse/Input", LocationType = LocationType.Internal, WarehouseId = warehouse.Id, IsActive = true };
        var outputLoc = new Location { Name = "WH/Output", NameAr = "المستودع/التسليم", CompleteName = "Main Warehouse/Output", LocationType = LocationType.Internal, WarehouseId = warehouse.Id, IsActive = true };
        context.Locations.AddRange(stockLoc, inputLoc, outputLoc);
        await context.SaveChangesAsync();

        warehouse.StockLocationId = stockLoc.Id;
        warehouse.InputLocationId = inputLoc.Id;
        warehouse.OutputLocationId = outputLoc.Id;
        await context.SaveChangesAsync();

        // 5. Stock Picking Types
        var pickingTypes = new List<StockPickingType>
        {
            new() { Name = "Receipts", NameAr = "استلامات", Code = PickingTypeCode.Incoming, Sequence = "IN/", WarehouseId = warehouse.Id, DefaultSourceLocationId = vendorLoc.Id, DefaultDestLocationId = inputLoc.Id, IsActive = true },
            new() { Name = "Delivery Orders", NameAr = "أوامر تسليم", Code = PickingTypeCode.Outgoing, Sequence = "OUT/", WarehouseId = warehouse.Id, DefaultSourceLocationId = stockLoc.Id, DefaultDestLocationId = customerLoc.Id, IsActive = true },
            new() { Name = "Internal Transfers", NameAr = "تحويلات داخلية", Code = PickingTypeCode.Internal, Sequence = "INT/", WarehouseId = warehouse.Id, DefaultSourceLocationId = stockLoc.Id, DefaultDestLocationId = stockLoc.Id, IsActive = true }
        };
        context.StockPickingTypes.AddRange(pickingTypes);
        await context.SaveChangesAsync();
    }
}
