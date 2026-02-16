using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Areas.Inventory.Controllers;

[Area("Inventory")]
[Authorize]
public class WarehouseController : Controller
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    public async Task<IActionResult> Index()
    {
        var warehouses = await _warehouseService.GetAllAsync();
        return View(warehouses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        if (warehouse == null) return NotFound();
        return View(warehouse);
    }

    public IActionResult Create()
    {
        return View(new Warehouse { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Warehouse warehouse)
    {
        try
        {
            var created = await _warehouseService.CreateAsync(warehouse);
            TempData["Success"] = "تم إنشاء المستودع بنجاح | Warehouse created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(warehouse);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        if (warehouse == null) return NotFound();
        return View(warehouse);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Warehouse warehouse)
    {
        try
        {
            await _warehouseService.UpdateAsync(warehouse);
            TempData["Success"] = "تم تحديث المستودع بنجاح | Warehouse updated successfully";
            return RedirectToAction(nameof(Details), new { id = warehouse.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(warehouse);
        }
    }
}
