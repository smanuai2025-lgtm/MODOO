using AiModoo.Core.Entities.Identity;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Infrastructure.Data;
using AiModoo.Infrastructure.Repositories;
using AiModoo.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AiModoo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Cookie settings
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/AccountManagement/Login";
            options.AccessDeniedPath = "/AccountManagement/AccessDenied";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        // Repository & UoW
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Common services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IEmailService, EmailService>();

        // Redis (optional - graceful fallback if not available)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(redisConnection);
                services.AddSingleton<IConnectionMultiplexer>(redis);
                services.AddSingleton<ICacheService>(sp =>
                    new CacheService(sp.GetRequiredService<IConnectionMultiplexer>()));
            }
            catch
            {
                // Redis not available - use no-op cache
                services.AddSingleton<ICacheService>(new CacheService());
            }
        }
        else
        {
            services.AddSingleton<ICacheService>(new CacheService());
        }

        return services;
    }
}
