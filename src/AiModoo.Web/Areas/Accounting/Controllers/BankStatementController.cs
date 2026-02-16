using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class BankStatementController : Controller
{
    private readonly IBankStatementService _bankStatementService;
    private readonly IBankReconciliationService _bankReconciliationService;
    private readonly IRepository<BankAccount> _bankAccountRepository;

    public BankStatementController(
        IBankStatementService bankStatementService,
        IBankReconciliationService bankReconciliationService,
        IRepository<BankAccount> bankAccountRepository)
    {
        _bankStatementService = bankStatementService;
        _bankReconciliationService = bankReconciliationService;
        _bankAccountRepository = bankAccountRepository;
    }

    public async Task<IActionResult> Index()
    {
        var statements = await _bankStatementService.GetAllAsync();
        return View(statements);
    }

    public async Task<IActionResult> Details(int id)
    {
        var statement = await _bankStatementService.GetByIdAsync(id);
        if (statement == null) return NotFound();
        return View(statement);
    }

    public async Task<IActionResult> Create()
    {
        var bankAccounts = await _bankAccountRepository.GetAllAsync();
        ViewBag.BankAccounts = new SelectList(bankAccounts.Where(b => b.IsActive), "Id", "Name");
        return View(new BankStatement { Date = DateTime.Now, PeriodStart = DateTime.Now, PeriodEnd = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BankStatement statement)
    {
        try
        {
            var created = await _bankStatementService.CreateAsync(statement);
            TempData["Success"] = "تم إنشاء كشف البنك بنجاح | Bank statement created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var bankAccounts = await _bankAccountRepository.GetAllAsync();
            ViewBag.BankAccounts = new SelectList(bankAccounts.Where(b => b.IsActive), "Id", "Name");
            return View(statement);
        }
    }

    public async Task<IActionResult> Import()
    {
        var bankAccounts = await _bankAccountRepository.GetAllAsync();
        ViewBag.BankAccounts = new SelectList(bankAccounts.Where(b => b.IsActive), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(int bankAccountId, IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError("", "يرجى اختيار ملف | Please select a file");
            var bankAccounts = await _bankAccountRepository.GetAllAsync();
            ViewBag.BankAccounts = new SelectList(bankAccounts.Where(b => b.IsActive), "Id", "Name");
            return View();
        }

        try
        {
            using var stream = csvFile.OpenReadStream();
            var statement = await _bankStatementService.ImportCsvAsync(bankAccountId, stream);
            TempData["Success"] = "تم استيراد كشف البنك بنجاح | Bank statement imported successfully";
            return RedirectToAction(nameof(Details), new { id = statement.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var bankAccounts = await _bankAccountRepository.GetAllAsync();
            ViewBag.BankAccounts = new SelectList(bankAccounts.Where(b => b.IsActive), "Id", "Name");
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoMatch(int id)
    {
        try
        {
            await _bankStatementService.AutoMatchAsync(id);
            TempData["Success"] = "تمت المطابقة التلقائية بنجاح | Auto match completed successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReconciliation(int id)
    {
        try
        {
            var reconciliation = await _bankReconciliationService.CreateFromStatementAsync(id);
            TempData["Success"] = "تم إنشاء المطابقة البنكية بنجاح | Bank reconciliation created successfully";
            return RedirectToAction("Details", "BankReconciliation", new { id = reconciliation.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
