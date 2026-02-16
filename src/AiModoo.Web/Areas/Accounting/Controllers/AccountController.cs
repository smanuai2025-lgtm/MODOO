using AiModoo.Core.DTOs.Accounting;
using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<IActionResult> Index()
    {
        var accounts = await _accountService.GetAllAsync();
        return View(accounts);
    }

    public async Task<IActionResult> Tree()
    {
        var tree = await _accountService.GetChartOfAccountsTreeAsync();
        var dtos = tree.Select(MapToDto).ToList();
        return View(dtos);
    }

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            NameAr = account.NameAr,
            AccountType = account.AccountType,
            ParentAccountId = account.ParentAccountId,
            Level = account.Level,
            IsActive = account.IsActive,
            Balance = 0,
            ChildAccounts = account.ChildAccounts?.Select(MapToDto).ToList()
        };
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Account());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Account account)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(account);
        }

        try
        {
            await _accountService.CreateAsync(account);
            TempData["Success"] = "تم إنشاء الحساب بنجاح | Account created successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(account);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return NotFound();
        await PopulateDropdowns();
        return View(account);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Account account)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(account);
        }

        try
        {
            await _accountService.UpdateAsync(account);
            TempData["Success"] = "تم تحديث الحساب بنجاح | Account updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(account);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _accountService.DeleteAsync(id);
            TempData["Success"] = "تم حذف الحساب بنجاح | Account deleted successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetBalance(int id)
    {
        var balance = await _accountService.GetBalanceAsync(id);
        return Json(new { balance });
    }

    private async Task PopulateDropdowns()
    {
        var accounts = await _accountService.GetAllAsync();
        ViewBag.ParentAccounts = new SelectList(accounts.Where(a => a.Level == 0), "Id", "Name");
        ViewBag.AccountTypes = new SelectList(Enum.GetValues<AccountTypeEnum>().Select(t => new { Value = (int)t, Text = t.ToString() }), "Value", "Text");
    }
}
