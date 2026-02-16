using AiModoo.Core.Constants;
using AiModoo.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AiModoo.Infrastructure.Data.Seeds;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Roles
        string[] roles = {
            Roles.Admin, Roles.Accountant, Roles.SalesManager, Roles.SalesUser,
            Roles.PurchaseManager, Roles.PurchaseUser, Roles.InventoryManager,
            Roles.InventoryUser, Roles.PosUser, Roles.HrManager, Roles.HrUser, Roles.Employee
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    IsSystemRole = true,
                    Description = $"{role} role",
                    DescriptionAr = $"دور {role}"
                });
            }
        }

        // Seed Admin User
        var adminEmail = "admin@aimodoo.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                FullNameAr = "مدير النظام",
                PreferredLanguage = "ar",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }
    }
}
