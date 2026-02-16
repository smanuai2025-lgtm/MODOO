using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Web.Areas.Sales.Controllers;

[Area("Sales")]
[Authorize]
public class PartnerController : Controller
{
    private readonly IPartnerService _partnerService;
    private readonly IPricelistService _pricelistService;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<PaymentTerm> _paymentTermRepository;

    public PartnerController(
        IPartnerService partnerService,
        IPricelistService pricelistService,
        IRepository<Account> accountRepository,
        IRepository<PaymentTerm> paymentTermRepository)
    {
        _partnerService = partnerService;
        _pricelistService = pricelistService;
        _accountRepository = accountRepository;
        _paymentTermRepository = paymentTermRepository;
    }

    public async Task<IActionResult> Index(PartnerType? type, string? search)
    {
        var partners = await _partnerService.GetAllAsync(type);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            partners = partners.Where(p =>
                p.Name.ToLower().Contains(s) ||
                (p.NameAr != null && p.NameAr.Contains(s)) ||
                (p.Email != null && p.Email.ToLower().Contains(s)) ||
                (p.Phone != null && p.Phone.Contains(s)));
        }

        ViewBag.CurrentType = type;
        ViewBag.CurrentSearch = search;
        return View(partners);
    }

    public async Task<IActionResult> Details(int id)
    {
        var partner = await _partnerService.GetByIdAsync(id);
        if (partner == null) return NotFound();
        ViewBag.OutstandingBalance = await _partnerService.GetOutstandingBalanceAsync(id);
        return View(partner);
    }

    public async Task<IActionResult> Create(PartnerType? type)
    {
        await PopulateDropdowns();
        return View(new Partner
        {
            PartnerType = type ?? PartnerType.Customer,
            IsActive = true,
            IsCompany = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Partner partner)
    {
        try
        {
            var created = await _partnerService.CreateAsync(partner);
            TempData["Success"] = "تم إنشاء الشريك بنجاح | Partner created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(partner);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var partner = await _partnerService.GetByIdAsync(id);
        if (partner == null) return NotFound();
        await PopulateDropdowns();
        return View(partner);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Partner partner)
    {
        try
        {
            await _partnerService.UpdateAsync(partner);
            TempData["Success"] = "تم تحديث الشريك بنجاح | Partner updated successfully";
            return RedirectToAction(nameof(Details), new { id = partner.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(partner);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _partnerService.DeleteAsync(id);
            TempData["Success"] = "تم حذف الشريك بنجاح | Partner deleted successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term, PartnerType? type)
    {
        var partners = await _partnerService.SearchAsync(term, type);
        var result = partners.Select(p => new
        {
            p.Id,
            p.Name,
            p.NameAr,
            p.Email,
            p.Phone,
            PartnerType = p.PartnerType.ToString()
        });
        return Json(result);
    }

    private async Task PopulateDropdowns()
    {
        var accounts = await _accountRepository.Query()
            .OrderBy(a => a.Code).ToListAsync();

        ViewBag.ReceivableAccounts = new SelectList(
            accounts.Where(a => a.AccountType == AccountTypeEnum.Asset),
            "Id", "Name");
        ViewBag.PayableAccounts = new SelectList(
            accounts.Where(a => a.AccountType == AccountTypeEnum.Liability),
            "Id", "Name");

        var paymentTerms = await _paymentTermRepository.GetAllAsync();
        ViewBag.PaymentTerms = new SelectList(paymentTerms, "Id", "Name");

        var pricelists = await _pricelistService.GetAllAsync();
        ViewBag.Pricelists = new SelectList(pricelists, "Id", "Name");
    }
}
