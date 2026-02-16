using AiModoo.Core.Entities.Purchases;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.Purchases;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Purchases.Controllers;

[Area("Purchases")]
[Authorize]
public class PurchaseOrderController : Controller
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly IPartnerService _partnerService;
    private readonly IProductService _productService;
    private readonly IRepository<PaymentTerm> _paymentTermRepository;

    public PurchaseOrderController(
        IPurchaseOrderService purchaseOrderService,
        IPartnerService partnerService,
        IProductService productService,
        IRepository<PaymentTerm> paymentTermRepository)
    {
        _purchaseOrderService = purchaseOrderService;
        _partnerService = partnerService;
        _productService = productService;
        _paymentTermRepository = paymentTermRepository;
    }

    public async Task<IActionResult> Index(PurchaseOrderStatus? status, string? search)
    {
        var orders = await _purchaseOrderService.GetAllAsync();

        if (status.HasValue)
            orders = orders.Where(o => o.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            orders = orders.Where(o =>
                o.Number.ToLower().Contains(s) ||
                o.Vendor.Name.ToLower().Contains(s));
        }

        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _purchaseOrderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new PurchaseOrder
        {
            OrderDate = DateTime.UtcNow
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseOrder order, int[] ProductIds, decimal[] Quantities, decimal[] UnitPrices, decimal[] Discounts, int?[] TaxIds)
    {
        try
        {
            order.Lines = new List<PurchaseOrderLine>();
            for (int i = 0; i < ProductIds.Length; i++)
            {
                if (ProductIds[i] == 0) continue;
                var product = await _productService.GetByIdAsync(ProductIds[i]);
                if (product == null) continue;

                order.Lines.Add(new PurchaseOrderLine
                {
                    Sequence = i + 1,
                    ProductId = ProductIds[i],
                    Description = product.Name,
                    DescriptionAr = product.NameAr,
                    Quantity = Quantities.Length > i ? Quantities[i] : 1,
                    UnitPrice = UnitPrices.Length > i ? UnitPrices[i] : product.Cost,
                    Discount = Discounts.Length > i ? Discounts[i] : 0,
                    TaxId = TaxIds.Length > i ? TaxIds[i] : null,
                    UomId = product.UomId
                });
            }

            var created = await _purchaseOrderService.CreateAsync(order);
            TempData["Success"] = "تم إنشاء طلب عرض السعر بنجاح | RFQ created successfully";
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
            await _purchaseOrderService.ConfirmAsync(id);
            TempData["Success"] = "تم تأكيد أمر الشراء بنجاح | Purchase order confirmed";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReceipt(int id)
    {
        try
        {
            await _purchaseOrderService.CreateReceiptAsync(id);
            TempData["Success"] = "تم إنشاء الاستلام بنجاح | Receipt created successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBill(int id)
    {
        try
        {
            await _purchaseOrderService.CreateBillAsync(id);
            TempData["Success"] = "تم إنشاء فاتورة المورد بنجاح | Vendor bill created successfully";
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
            await _purchaseOrderService.CancelAsync(id);
            TempData["Success"] = "تم إلغاء أمر الشراء | Purchase order cancelled";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateDropdowns()
    {
        var vendors = await _partnerService.GetAllAsync(PartnerType.Vendor);
        ViewBag.Vendors = new SelectList(vendors, "Id", "Name");

        var paymentTerms = await _paymentTermRepository.GetAllAsync();
        ViewBag.PaymentTerms = new SelectList(paymentTerms, "Id", "Name");

        var products = await _productService.GetAllAsync();
        ViewBag.Products = products.Where(p => p.CanBePurchased);
    }
}
