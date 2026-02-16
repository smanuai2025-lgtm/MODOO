using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class BankReconciliationController : Controller
{
    private readonly IBankReconciliationService _reconciliationService;
    private readonly IRepository<BankAccount> _bankAccountRepository;

    public BankReconciliationController(
        IBankReconciliationService reconciliationService,
        IRepository<BankAccount> bankAccountRepository)
    {
        _reconciliationService = reconciliationService;
        _bankAccountRepository = bankAccountRepository;
    }

    public async Task<IActionResult> Index()
    {
        var reconciliations = await _reconciliationService.GetAllAsync();
        return View(reconciliations);
    }

    public async Task<IActionResult> Details(int id)
    {
        var reconciliation = await _reconciliationService.GetByIdAsync(id);
        if (reconciliation == null) return NotFound();

        var unreconciledLines = await _reconciliationService.GetUnreconciledLinesAsync(reconciliation.BankAccountId);
        ViewBag.UnreconciledLines = unreconciledLines;
        return View(reconciliation);
    }

    public async Task<IActionResult> Create()
    {
        var bankAccounts = await _bankAccountRepository.GetAllAsync();
        ViewBag.BankAccounts = new SelectList(bankAccounts.Where(b => b.IsActive), "Id", "Name");
        return View(new BankReconciliation { Date = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BankReconciliation reconciliation)
    {
        try
        {
            var created = await _reconciliationService.CreateAsync(reconciliation);
            TempData["Success"] = "تم إنشاء المطابقة البنكية بنجاح | Bank reconciliation created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var bankAccounts = await _bankAccountRepository.GetAllAsync();
            ViewBag.BankAccounts = new SelectList(bankAccounts.Where(b => b.IsActive), "Id", "Name");
            return View(reconciliation);
        }
    }

    [HttpPost]
    public async Task<IActionResult> MatchLine(int reconciliationId, int lineId, int journalEntryLineId)
    {
        try
        {
            await _reconciliationService.MatchLineAsync(reconciliationId, lineId, journalEntryLineId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UnmatchLine(int reconciliationId, int lineId)
    {
        try
        {
            await _reconciliationService.UnmatchLineAsync(reconciliationId, lineId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(int id)
    {
        try
        {
            await _reconciliationService.ValidateAsync(id);
            TempData["Success"] = "تم اعتماد المطابقة البنكية بنجاح | Bank reconciliation validated successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
