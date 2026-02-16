using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Inventory.Controllers;

[Area("Inventory")]
[Authorize]
public class StockPickingController : Controller
{
    private readonly IStockPickingService _pickingService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;

    public StockPickingController(
        IStockPickingService pickingService,
        IProductService productService,
        IWarehouseService warehouseService)
    {
        _pickingService = pickingService;
        _productService = productService;
        _warehouseService = warehouseService;
    }

    public async Task<IActionResult> Index(PickingTypeCode? type, StockPickingStatus? status)
    {
        var pickings = await _pickingService.GetAllAsync(type, status);
        ViewBag.CurrentType = type;
        ViewBag.CurrentStatus = status;
        return View(pickings);
    }

    public async Task<IActionResult> Details(int id)
    {
        var picking = await _pickingService.GetByIdAsync(id);
        if (picking == null) return NotFound();
        return View(picking);
    }

    public async Task<IActionResult> Create(PickingTypeCode? type)
    {
        await PopulateDropdowns();
        var pickingTypes = await _pickingService.GetPickingTypesAsync();
        var defaultType = type.HasValue
            ? pickingTypes.FirstOrDefault(t => t.Code == type.Value)
            : pickingTypes.FirstOrDefault();

        var picking = new StockPicking
        {
            PickingTypeId = defaultType?.Id ?? 0,
            SourceLocationId = defaultType?.DefaultSourceLocationId ?? 0,
            DestLocationId = defaultType?.DefaultDestLocationId ?? 0,
            ScheduledDate = DateTime.Today,
            Moves = new List<StockMove> { new StockMove { ProductUomQty = 1 } }
        };
        return View(picking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockPicking picking)
    {
        try
        {
            picking.Moves = picking.Moves?.Where(m => m.ProductId > 0 && m.ProductUomQty > 0).ToList()
                ?? new List<StockMove>();

            var created = await _pickingService.CreateAsync(picking);
            TempData["Success"] = "تم إنشاء العملية بنجاح | Operation created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(picking);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            await _pickingService.ConfirmAsync(id);
            TempData["Success"] = "تم تأكيد العملية بنجاح | Operation confirmed successfully";
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
            await _pickingService.ValidateAsync(id);
            TempData["Success"] = "تم تنفيذ العملية بنجاح وتحديث المخزون | Operation validated and stock updated successfully";
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
            await _pickingService.CancelAsync(id);
            TempData["Success"] = "تم إلغاء العملية | Operation cancelled";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateDropdowns()
    {
        var pickingTypes = await _pickingService.GetPickingTypesAsync();
        var products = await _productService.GetAllAsync();
        var locations = await _warehouseService.GetLocationsAsync();

        ViewBag.PickingTypes = new SelectList(pickingTypes, "Id", "Name");
        ViewBag.Products = new SelectList(products.Select(p => new { p.Id, Name = $"[{p.InternalReference}] {p.Name}" }), "Id", "Name");
        ViewBag.Locations = new SelectList(locations.Select(l => new { l.Id, Name = l.CompleteName }), "Id", "Name");
    }
}
