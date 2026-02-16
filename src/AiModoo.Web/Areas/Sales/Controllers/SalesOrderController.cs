using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Web.Areas.Sales.Controllers;

[Area("Sales")]
[Authorize]
public class SalesOrderController : Controller
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly IPartnerService _partnerService;
    private readonly IPricelistService _pricelistService;
    private readonly IProductService _productService;
    private readonly IRepository<PaymentTerm> _paymentTermRepository;

    public SalesOrderController(
        ISalesOrderService salesOrderService,
        IPartnerService partnerService,
        IPricelistService pricelistService,
        IProductService productService,
        IRepository<PaymentTerm> paymentTermRepository)
    {
        _salesOrderService = salesOrderService;
        _partnerService = partnerService;
        _pricelistService = pricelistService;
        _productService = productService;
        _paymentTermRepository = paymentTermRepository;
    }

    public async Task<IActionResult> Index(SalesOrderStatus? status, string? search)
    {
        var orders = await _salesOrderService.GetAllAsync();

        if (status.HasValue)
            orders = orders.Where(o => o.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            orders = orders.Where(o =>
                o.Number.ToLower().Contains(s) ||
                o.Partner.Name.ToLower().Contains(s));
        }

        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _salesOrderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new SalesOrder
        {
            OrderDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            InvoicingPolicy = InvoicingPolicy.OrderedQuantities
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SalesOrder order, int[] ProductIds, decimal[] Quantities, decimal[] UnitPrices, decimal[] Discounts, int?[] TaxIds)
    {
        try
        {
            order.Lines = new List<SalesOrderLine>();
            for (int i = 0; i < ProductIds.Length; i++)
            {
                if (ProductIds[i] == 0) continue;
                var product = await _productService.GetByIdAsync(ProductIds[i]);
                if (product == null) continue;

                order.Lines.Add(new SalesOrderLine
                {
                    Sequence = i + 1,
                    ProductId = ProductIds[i],
                    Description = product.Name,
                    DescriptionAr = product.NameAr,
                    Quantity = Quantities.Length > i ? Quantities[i] : 1,
                    UnitPrice = UnitPrices.Length > i ? UnitPrices[i] : product.SalePrice,
                    Discount = Discounts.Length > i ? Discounts[i] : 0,
                    TaxId = TaxIds.Length > i ? TaxIds[i] : null,
                    UomId = product.UomId
                });
            }

            var created = await _salesOrderService.CreateAsync(order);
            TempData["Success"] = "تم إنشاء عرض السعر بنجاح | Quotation created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(order);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            await _salesOrderService.ConfirmAsync(id);
            TempData["Success"] = "تم تأكيد الطلب بنجاح | Order confirmed successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDelivery(int id)
    {
        try
        {
            await _salesOrderService.CreateDeliveryAsync(id);
            TempData["Success"] = "تم إنشاء التسليم بنجاح | Delivery created successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvoice(int id)
    {
        try
        {
            await _salesOrderService.CreateInvoiceAsync(id);
            TempData["Success"] = "تم إنشاء الفاتورة بنجاح | Invoice created successfully";
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
            await _salesOrderService.CancelAsync(id);
            TempData["Success"] = "تم إلغاء الطلب | Order cancelled";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetProductPrice(int productId, int? pricelistId, decimal qty = 1)
    {
        if (pricelistId.HasValue)
        {
            var price = await _pricelistService.GetProductPriceAsync(pricelistId.Value, productId, qty);
            return Json(new { price });
        }

        var product = await _productService.GetByIdAsync(productId);
        return Json(new { price = product?.SalePrice ?? 0 });
    }

    private async Task PopulateDropdowns()
    {
        var customers = await _partnerService.GetAllAsync(PartnerType.Customer);
        ViewBag.Customers = new SelectList(customers, "Id", "Name");

        var paymentTerms = await _paymentTermRepository.GetAllAsync();
        ViewBag.PaymentTerms = new SelectList(paymentTerms, "Id", "Name");

        var pricelists = await _pricelistService.GetAllAsync();
        ViewBag.Pricelists = new SelectList(pricelists, "Id", "Name");

        var products = await _productService.GetAllAsync();
        ViewBag.Products = products.Where(p => p.CanBeSold);
    }
}
