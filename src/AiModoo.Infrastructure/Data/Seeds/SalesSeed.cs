using AiModoo.Core.Entities.Sales;
using AiModoo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiModoo.Infrastructure.Data.Seeds;

public static class SalesSeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Partners.AnyAsync()) return;

        // Payment Terms
        var paymentTerms = new List<PaymentTerm>
        {
            new()
            {
                Name = "Immediate Payment", NameAr = "دفع فوري",
                Lines = new List<PaymentTermLine> { new() { Sequence = 1, Percentage = 100, Days = 0 } }
            },
            new()
            {
                Name = "Net 15", NameAr = "صافي 15 يوم",
                Lines = new List<PaymentTermLine> { new() { Sequence = 1, Percentage = 100, Days = 15 } }
            },
            new()
            {
                Name = "Net 30", NameAr = "صافي 30 يوم",
                Lines = new List<PaymentTermLine> { new() { Sequence = 1, Percentage = 100, Days = 30 } }
            },
            new()
            {
                Name = "Net 60", NameAr = "صافي 60 يوم",
                Lines = new List<PaymentTermLine> { new() { Sequence = 1, Percentage = 100, Days = 60 } }
            },
            new()
            {
                Name = "30% Now, 70% in 30 Days", NameAr = "30% مقدم و70% بعد 30 يوم",
                Lines = new List<PaymentTermLine>
                {
                    new() { Sequence = 1, Percentage = 30, Days = 0 },
                    new() { Sequence = 2, Percentage = 70, Days = 30 }
                }
            }
        };

        context.PaymentTerms.AddRange(paymentTerms);
        await context.SaveChangesAsync();

        // Pricelists
        var publicPricelist = new Pricelist { Name = "Public Pricelist", NameAr = "قائمة الأسعار العامة", IsActive = true };
        var vipPricelist = new Pricelist { Name = "VIP Pricelist", NameAr = "قائمة أسعار VIP", IsActive = true };
        context.Pricelists.AddRange(publicPricelist, vipPricelist);
        await context.SaveChangesAsync();

        // Sales Teams
        var salesTeam = new SalesTeam { Name = "Direct Sales", NameAr = "مبيعات مباشرة" };
        var onlineTeam = new SalesTeam { Name = "Online Sales", NameAr = "مبيعات إلكترونية" };
        context.SalesTeams.AddRange(salesTeam, onlineTeam);
        await context.SaveChangesAsync();
    }
}
