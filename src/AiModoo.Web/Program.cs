using AiModoo.Infrastructure;
using AiModoo.Infrastructure.Data;
using AiModoo.Infrastructure.Data.Seeds;
using AiModoo.Services;
using AiModoo.Web.Middleware;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure services (DB, Identity, Redis, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application services (AutoMapper, FluentValidation, Business services)
builder.Services.AddApplicationServices();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// MVC with localization
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// SignalR for POS
builder.Services.AddSignalR();

// HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Localization middleware
var supportedCultures = new[]
{
    new CultureInfo("ar-SA"),
    new CultureInfo("ar"),
    new CultureInfo("en-US"),
    new CultureInfo("en")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ar-SA"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
});

// Custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Route mapping
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await IdentitySeed.SeedAsync(services);
        await AccountingSeed.SeedAsync(services);
        await InventorySeed.SeedAsync(services);
        await SalesSeed.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
