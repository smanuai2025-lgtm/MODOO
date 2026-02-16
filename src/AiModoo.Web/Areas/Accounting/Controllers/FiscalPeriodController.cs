using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Interfaces.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class FiscalPeriodController : Controller
{
    private readonly IFiscalPeriodService _fiscalPeriodService;

    public FiscalPeriodController(IFiscalPeriodService fiscalPeriodService)
    {
        _fiscalPeriodService = fiscalPeriodService;
    }

    public async Task<IActionResult> Index()
    {
        var years = await _fiscalPeriodService.GetAllYearsAsync();
        return View(years);
    }

    public IActionResult Create()
    {
        return View(new FiscalYear { StartDate = new DateTime(DateTime.Now.Year, 1, 1), EndDate = new DateTime(DateTime.Now.Year, 12, 31) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FiscalYear year)
    {
        try
        {
            await _fiscalPeriodService.CreateYearAsync(year);
            TempData["Success"] = "تم إنشاء السنة المالية بنجاح | Fiscal year created successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(year);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClosePeriod(int id)
    {
        try
        {
            await _fiscalPeriodService.ClosePeriodAsync(id);
            TempData["Success"] = "تم إغلاق الفترة بنجاح | Period closed successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseYear(int id)
    {
        try
        {
            await _fiscalPeriodService.CloseYearAsync(id);
            TempData["Success"] = "تم إغلاق السنة المالية بنجاح | Fiscal year closed successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateClosingEntry(int id)
    {
        try
        {
            var entry = await _fiscalPeriodService.GenerateClosingEntryAsync(id);
            TempData["Success"] = $"تم إنشاء قيد الإقفال بنجاح ({entry.Number}) | Closing entry created successfully ({entry.Number})";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
