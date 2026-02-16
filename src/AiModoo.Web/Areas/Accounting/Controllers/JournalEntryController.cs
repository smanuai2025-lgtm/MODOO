using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class JournalEntryController : Controller
{
    private readonly IJournalEntryService _journalEntryService;
    private readonly IAccountService _accountService;
    private readonly IFiscalPeriodService _fiscalPeriodService;
    private readonly Core.Interfaces.Common.IRepository<Journal> _journalRepository;

    public JournalEntryController(
        IJournalEntryService journalEntryService,
        IAccountService accountService,
        IFiscalPeriodService fiscalPeriodService,
        Core.Interfaces.Common.IRepository<Journal> journalRepository)
    {
        _journalEntryService = journalEntryService;
        _accountService = accountService;
        _fiscalPeriodService = fiscalPeriodService;
        _journalRepository = journalRepository;
    }

    public async Task<IActionResult> Index(JournalEntryStatus? status)
    {
        var entries = await _journalEntryService.GetAllAsync();
        if (status.HasValue)
            entries = entries.Where(e => e.Status == status.Value).ToList();
        ViewBag.CurrentStatus = status;
        return View(entries);
    }

    public async Task<IActionResult> Details(int id)
    {
        var entry = await _journalEntryService.GetByIdAsync(id);
        if (entry == null) return NotFound();
        return View(entry);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        var entry = new JournalEntry
        {
            Date = DateTime.Now,
            Lines = new List<JournalEntryLine> { new(), new() }
        };
        return View(entry);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JournalEntry entry)
    {
        // Remove empty lines
        entry.Lines = entry.Lines?.Where(l => l.AccountId > 0 && (l.Debit > 0 || l.Credit > 0)).ToList()
            ?? new List<JournalEntryLine>();

        try
        {
            var created = await _journalEntryService.CreateAsync(entry);
            TempData["Success"] = "تم إنشاء القيد بنجاح | Journal entry created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(entry);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(int id)
    {
        try
        {
            await _journalEntryService.PostAsync(id);
            TempData["Success"] = "تم ترحيل القيد بنجاح | Journal entry posted successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _journalEntryService.CancelAsync(id);
            TempData["Success"] = "تم إلغاء القيد | Journal entry cancelled";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reverse(int id, string reason)
    {
        try
        {
            var reversal = await _journalEntryService.ReverseAsync(id, DateTime.Today);
            TempData["Success"] = $"تم عكس القيد - القيد الجديد: {reversal.Number} | Entry reversed - New entry: {reversal.Number}";
            return RedirectToAction(nameof(Details), new { id = reversal.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateDropdowns()
    {
        var accounts = await _accountService.GetAllAsync();
        var journals = await _journalRepository.GetAllAsync();
        ViewBag.Accounts = new SelectList(accounts.Where(a => a.IsActive), "Id", "Name");
        ViewBag.Journals = new SelectList(journals.Where(j => j.IsActive), "Id", "Name");
    }
}
