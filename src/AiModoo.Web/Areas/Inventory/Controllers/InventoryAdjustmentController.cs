using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Inventory.Controllers;

[Area("Inventory")]
[Authorize]
public class InventoryAdjustmentController : Controller
{
    private readonly IInventoryAdjustmentService _adjustmentService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;

    public InventoryAdjustmentController(
        IInventoryAdjustmentService adjustmentService,
        IProductService productService,
        IWarehouseService warehouseService)
    {
        _adjustmentService = adjustmentService;
        _productService = productService;
        _warehouseService = warehouseService;
    }

    public async Task<IActionResult> Index()
    {
        var adjustments = await _adjustmentService.GetAllAsync();
        return View(adjustments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var adjustment = await _adjustmentService.GetByIdAsync(id);
        if (adjustment == null) return NotFound();
        return View(adjustment);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        var adjustment = new InventoryAdjustment
        {
            Date = DateTime.Today,
            Lines = new List<InventoryAdjustmentLine> { new InventoryAdjustmentLine() }
        };
        return View(adjustment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryAdjustment adjustment)
    {
        try
        {
            adjustment.Lines = adjustment.Lines?.Where(l => l.ProductId > 0).ToList()
                ?? new List<InventoryAdjustmentLine>();

            var created = await _adjustmentService.CreateAsync(adjustment);
            TempData["Success"] = "تم إنشاء التسوية بنجاح | Adjustment created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(adjustment);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartCount(int id)
    {
        try
        {
            await _adjustmentService.StartCountAsync(id);
            TempData["Success"] = "تم بدء الجرد | Counting started";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(int id)
    {
        try
        {
            await _adjustmentService.ValidateAsync(id);
            TempData["Success"] = "تم التحقق من التسوية وتحديث المخزون | Adjustment validated and stock updated";
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
            await _adjustmentService.CancelAsync(id);
            TempData["Success"] = "تم إلغاء التسوية | Adjustment cancelled";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateDropdowns()
    {
        var warehouses = await _warehouseService.GetAllAsync();
        var products = await _productService.GetAllAsync();
        var locations = await _warehouseService.GetLocationsAsync();

        ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");
        ViewBag.Products = new SelectList(products.Select(p => new { p.Id, Name = $"[{p.InternalReference}] {p.Name}" }), "Id", "Name");
        ViewBag.Locations = new SelectList(locations.Where(l => l.LocationType == Core.Enums.LocationType.Internal).Select(l => new { l.Id, Name = l.CompleteName }), "Id", "Name");
    }
}
