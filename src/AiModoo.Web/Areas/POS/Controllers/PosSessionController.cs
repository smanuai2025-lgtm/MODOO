using AiModoo.Core.Entities.POS;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.POS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.POS.Controllers;

[Area("POS")]
[Authorize]
public class PosSessionController : Controller
{
    private readonly IPosService _posService;
    private readonly IProductService _productService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountingIntegrationService _accountingIntegration;
    private readonly IRepository<Core.Entities.Accounting.PaymentMethod> _paymentMethodRepository;

    public PosSessionController(
        IPosService posService,
        IProductService productService,
        ICurrentUserService currentUserService,
        IAccountingIntegrationService accountingIntegration,
        IRepository<Core.Entities.Accounting.PaymentMethod> paymentMethodRepository)
    {
        _posService = posService;
        _productService = productService;
        _currentUserService = currentUserService;
        _accountingIntegration = accountingIntegration;
        _paymentMethodRepository = paymentMethodRepository;
    }

    // GET: /POS/PosSession
    public async Task<IActionResult> Index()
    {
        var sessions = await _posService.GetSessionsAsync();
        return View(sessions);
    }

    // GET: /POS/PosSession/Open
    public async Task<IActionResult> Open()
    {
        var configs = await _posService.GetConfigsAsync();
        ViewBag.Configs = new SelectList(configs.Where(c => c.IsActive), "Id", "Name");
        return View();
    }

    // POST: /POS/PosSession/Open
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(int configId, decimal openingBalance)
    {
        try
        {
            var userId = _currentUserService.UserId ?? "system";
            var userName = _currentUserService.UserName ?? "System";
            var session = await _posService.OpenSessionAsync(configId, userId, userName, openingBalance);
            return RedirectToAction(nameof(Terminal), new { id = session.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var configs = await _posService.GetConfigsAsync();
            ViewBag.Configs = new SelectList(configs.Where(c => c.IsActive), "Id", "Name");
            return View();
        }
    }

    // GET: /POS/PosSession/Terminal/{id}
    public async Task<IActionResult> Terminal(int id)
    {
        var session = await _posService.GetSessionByIdAsync(id);
        if (session == null) return NotFound();
        if (session.Status != PosSessionStatus.Opened)
        {
            TempData["Error"] = "هذه الجلسة غير مفتوحة | This session is not open.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var products = (await _productService.GetAllAsync())
            .Where(p => p.CanBeSold && p.IsActive)
            .ToList();
        var paymentMethods = await _paymentMethodRepository.GetAllAsync();

        ViewBag.Products = products;
        ViewBag.PaymentMethods = paymentMethods.Where(pm => pm.IsActive).ToList();
        return View(session);
    }

    // POST: /POS/PosSession/CreateOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(int sessionId, int[] ProductIds, string[] ProductNames,
        decimal[] Quantities, decimal[] UnitPrices, decimal[] Discounts, decimal[] TaxAmounts,
        int[] PaymentMethodIds, decimal[] PaymentAmounts, decimal amountPaid, string? partnerName, string? notes)
    {
        try
        {
            var order = new PosOrder
            {
                SessionId = sessionId,
                PartnerName = partnerName,
                AmountPaid = amountPaid,
                Notes = notes,
                Lines = new List<PosOrderLine>(),
                Payments = new List<PosPayment>()
            };

            for (int i = 0; i < ProductIds.Length; i++)
            {
                if (ProductIds[i] == 0) continue;
                order.Lines.Add(new PosOrderLine
                {
                    ProductId = ProductIds[i],
                    ProductName = ProductNames.Length > i ? ProductNames[i] : "",
                    Quantity = Quantities.Length > i ? Quantities[i] : 1,
                    UnitPrice = UnitPrices.Length > i ? UnitPrices[i] : 0,
                    Discount = Discounts.Length > i ? Discounts[i] : 0,
                    TaxAmount = TaxAmounts.Length > i ? TaxAmounts[i] : 0
                });
            }

            for (int i = 0; i < PaymentMethodIds.Length; i++)
            {
                if (PaymentMethodIds[i] == 0 || (PaymentAmounts.Length > i && PaymentAmounts[i] <= 0)) continue;
                order.Payments.Add(new PosPayment
                {
                    PaymentMethodId = PaymentMethodIds[i],
                    Amount = PaymentAmounts.Length > i ? PaymentAmounts[i] : 0
                });
            }

            var created = await _posService.CreateOrderAsync(order);
            return Json(new { success = true, orderId = created.Id, orderNumber = created.Number, total = created.Total, change = created.Change });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: /POS/PosSession/Close
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int sessionId, decimal closingBalance, string? notes)
    {
        try
        {
            await _posService.CloseSessionAsync(sessionId, closingBalance, notes);

            try
            {
                await _accountingIntegration.PostPosSessionAsync(sessionId);
            }
            catch
            {
                // Accounting posting is optional - session still closes
            }

            TempData["Success"] = "تم إغلاق الجلسة بنجاح | Session closed successfully.";
            return RedirectToAction(nameof(Details), new { id = sessionId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Terminal), new { id = sessionId });
        }
    }

    // GET: /POS/PosSession/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var session = await _posService.GetSessionByIdAsync(id);
        if (session == null) return NotFound();
        return View(session);
    }
}
