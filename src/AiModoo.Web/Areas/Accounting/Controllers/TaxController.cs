using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class TaxController : Controller
{
    private readonly ITaxService _taxService;
    private readonly IAccountService _accountService;

    public TaxController(ITaxService taxService, IAccountService accountService)
    {
        _taxService = taxService;
        _accountService = accountService;
    }

    public async Task<IActionResult> Index()
    {
        var taxes = await _taxService.GetAllActiveAsync();
        return View(taxes);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Tax());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tax tax)
    {
        try
        {
            await _taxService.CreateAsync(tax);
            TempData["Success"] = "تم إنشاء الضريبة بنجاح | Tax created successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(tax);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var tax = await _taxService.GetByIdAsync(id);
        if (tax == null) return NotFound();
        await PopulateDropdowns();
        return View(tax);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Tax tax)
    {
        try
        {
            await _taxService.UpdateAsync(tax);
            TempData["Success"] = "تم تحديث الضريبة بنجاح | Tax updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(tax);
        }
    }

    private async Task PopulateDropdowns()
    {
        var accounts = await _accountService.GetAllAsync();
        ViewBag.Accounts = new SelectList(accounts.Where(a => a.IsActive), "Id", "Name");
        ViewBag.TaxTypes = new SelectList(Enum.GetValues<TaxType>().Select(t => new { Value = (int)t, Text = t.ToString() }), "Value", "Text");
    }
}
