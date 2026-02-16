using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class InvoiceController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Tax> _taxRepository;
    private readonly IRepository<Currency> _currencyRepository;
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;

    public InvoiceController(
        IInvoiceService invoiceService,
        IPaymentService paymentService,
        IRepository<Journal> journalRepository,
        IRepository<Account> accountRepository,
        IRepository<Tax> taxRepository,
        IRepository<Currency> currencyRepository,
        IRepository<PaymentMethod> paymentMethodRepository)
    {
        _invoiceService = invoiceService;
        _paymentService = paymentService;
        _journalRepository = journalRepository;
        _accountRepository = accountRepository;
        _taxRepository = taxRepository;
        _currencyRepository = currencyRepository;
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<IActionResult> Index(InvoiceType? type, InvoiceStatus? status)
    {
        var invoices = await _invoiceService.GetAllAsync(type, status);
        ViewBag.CurrentType = type;
        ViewBag.CurrentStatus = status;
        return View(invoices);
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        await PopulatePaymentDropdowns();
        return View(invoice);
    }

    public async Task<IActionResult> Create(InvoiceType? type)
    {
        await PopulateDropdowns();
        var invoice = new Invoice
        {
            InvoiceType = type ?? InvoiceType.CustomerInvoice,
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Lines = new List<InvoiceLine>
            {
                new InvoiceLine { Sequence = 1, Quantity = 1 }
            }
        };
        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Invoice invoice)
    {
        try
        {
            // Remove empty lines
            invoice.Lines = invoice.Lines?.Where(l =>
                !string.IsNullOrWhiteSpace(l.Description) || l.UnitPrice != 0 || l.AccountId.HasValue
            ).ToList() ?? new List<InvoiceLine>();

            var created = await _invoiceService.CreateAsync(invoice);
            TempData["Success"] = "تم إنشاء الفاتورة بنجاح | Invoice created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(invoice);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != InvoiceStatus.Draft)
        {
            TempData["Error"] = "لا يمكن تعديل فاتورة غير مسودة | Only draft invoices can be edited";
            return RedirectToAction(nameof(Details), new { id });
        }
        await PopulateDropdowns();
        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Invoice invoice)
    {
        try
        {
            invoice.Lines = invoice.Lines?.Where(l =>
                !string.IsNullOrWhiteSpace(l.Description) || l.UnitPrice != 0 || l.AccountId.HasValue
            ).ToList() ?? new List<InvoiceLine>();

            await _invoiceService.UpdateAsync(invoice);
            TempData["Success"] = "تم تحديث الفاتورة بنجاح | Invoice updated successfully";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(invoice);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            await _invoiceService.ConfirmAsync(id);
            TempData["Success"] = "تم تأكيد الفاتورة وترحيلها بنجاح | Invoice confirmed and posted successfully";
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
            await _invoiceService.CancelAsync(id);
            TempData["Success"] = "تم إلغاء الفاتورة | Invoice cancelled";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCreditNote(int id)
    {
        try
        {
            var creditNote = await _invoiceService.CreateCreditNoteAsync(id);
            TempData["Success"] = "تم إنشاء إشعار دائن بنجاح | Credit note created successfully";
            return RedirectToAction(nameof(Details), new { id = creditNote.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPayment(int invoiceId, decimal amount, int paymentMethodId, int journalId, string? memo)
    {
        try
        {
            // Create and post a payment
            var payment = new Payment
            {
                Date = DateTime.Today,
                Amount = amount,
                PaymentMethodId = paymentMethodId,
                JournalId = journalId,
                Memo = memo ?? $"Payment for Invoice",
                PaymentType = PaymentType.Inbound
            };

            // Get invoice to determine payment type
            var invoice = await _invoiceService.GetByIdAsync(invoiceId);
            if (invoice != null)
            {
                payment.PartnerId = invoice.PartnerId;
                payment.PartnerType = invoice.PartnerType;
                payment.PartnerName = invoice.PartnerName;
                payment.PaymentType = invoice.InvoiceType is InvoiceType.CustomerInvoice or InvoiceType.CustomerCreditNote
                    ? PaymentType.Inbound
                    : PaymentType.Outbound;
            }

            var created = await _paymentService.CreateAsync(payment);
            await _paymentService.PostAsync(created.Id);
            await _paymentService.AllocateToInvoiceAsync(created.Id, invoiceId, amount);

            TempData["Success"] = "تم تسجيل الدفعة بنجاح | Payment registered successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = invoiceId });
    }

    private async Task PopulateDropdowns()
    {
        var journals = await _journalRepository.GetAllAsync();
        var accounts = await _accountRepository.Query().Where(a => a.IsActive).OrderBy(a => a.Code).ToListAsync();
        var taxes = await _taxRepository.Query().Where(t => t.IsActive).ToListAsync();
        var currencies = await _currencyRepository.GetAllAsync();

        ViewBag.Journals = new SelectList(journals.Where(j => j.IsActive), "Id", "Name");
        ViewBag.Accounts = new SelectList(accounts.Select(a => new { a.Id, Name = $"{a.Code} - {a.Name}" }), "Id", "Name");
        ViewBag.Taxes = new SelectList(taxes, "Id", "Name");
        ViewBag.Currencies = new SelectList(currencies.Where(c => c.IsActive), "Id", "Name");
    }

    private async Task PopulatePaymentDropdowns()
    {
        var journals = await _journalRepository.GetAllAsync();
        var methods = await _paymentMethodRepository.GetAllAsync();
        ViewBag.PaymentJournals = new SelectList(journals.Where(j => j.IsActive && (j.Type == JournalTypeEnum.Cash || j.Type == JournalTypeEnum.Bank)), "Id", "Name");
        ViewBag.PaymentMethods = new SelectList(methods.Where(m => m.IsActive), "Id", "Name");
    }
}
