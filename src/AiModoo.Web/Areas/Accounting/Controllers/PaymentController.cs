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
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;
    private readonly IRepository<Currency> _currencyRepository;

    public PaymentController(
        IPaymentService paymentService,
        IRepository<Journal> journalRepository,
        IRepository<PaymentMethod> paymentMethodRepository,
        IRepository<Currency> currencyRepository)
    {
        _paymentService = paymentService;
        _journalRepository = journalRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<IActionResult> Index(PaymentType? type)
    {
        var payments = await _paymentService.GetAllAsync();
        if (type.HasValue)
            payments = payments.Where(p => p.PaymentType == type.Value).ToList();
        ViewBag.CurrentType = type;
        return View(payments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null) return NotFound();
        return View(payment);
    }

    public async Task<IActionResult> Create(PaymentType? type)
    {
        await PopulateDropdowns();
        var payment = new Payment
        {
            Date = DateTime.Now,
            PaymentType = type ?? PaymentType.Inbound
        };
        return View(payment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Payment payment)
    {
        try
        {
            var created = await _paymentService.CreateAsync(payment);
            TempData["Success"] = "تم إنشاء الدفعة بنجاح | Payment created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(payment);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(int id)
    {
        try
        {
            await _paymentService.PostAsync(id);
            TempData["Success"] = "تم ترحيل الدفعة بنجاح | Payment posted successfully";
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
            await _paymentService.CancelAsync(id);
            TempData["Success"] = "تم إلغاء الدفعة | Payment cancelled";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateDropdowns()
    {
        var journals = await _journalRepository.GetAllAsync();
        var methods = await _paymentMethodRepository.GetAllAsync();
        var currencies = await _currencyRepository.GetAllAsync();
        ViewBag.Journals = new SelectList(journals.Where(j => j.IsActive && (j.Type == JournalTypeEnum.Cash || j.Type == JournalTypeEnum.Bank)), "Id", "Name");
        ViewBag.PaymentMethods = new SelectList(methods.Where(m => m.IsActive), "Id", "Name");
        ViewBag.Currencies = new SelectList(currencies.Where(c => c.IsActive), "Id", "Name");
        ViewBag.PaymentTypes = new SelectList(Enum.GetValues<PaymentType>().Select(t => new { Value = (int)t, Text = t.ToString() }), "Value", "Text");
    }
}
