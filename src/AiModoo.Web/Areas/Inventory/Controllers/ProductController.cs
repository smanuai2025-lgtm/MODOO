using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Inventory.Controllers;

[Area("Inventory")]
[Authorize]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly IStockQuantService _stockQuantService;

    public ProductController(
        IProductService productService,
        IWarehouseService warehouseService,
        IStockQuantService stockQuantService)
    {
        _productService = productService;
        _warehouseService = warehouseService;
        _stockQuantService = stockQuantService;
    }

    public async Task<IActionResult> Index(ProductType? type, int? categoryId, string? search)
    {
        var products = await _productService.GetAllAsync(type, categoryId, search);
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        ViewBag.CurrentType = type;
        ViewBag.CurrentCategory = categoryId;
        ViewBag.CurrentSearch = search;
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

        ViewBag.OnHand = await _stockQuantService.GetOnHandAsync(id);
        ViewBag.Available = await _stockQuantService.GetAvailableAsync(id);
        return View(product);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Product { Type = ProductType.Storable, IsActive = true, CanBeSold = true, CanBePurchased = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        try
        {
            var created = await _productService.CreateAsync(product);
            TempData["Success"] = "تم إنشاء المنتج بنجاح | Product created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(product);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();
        await PopulateDropdowns();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product)
    {
        try
        {
            await _productService.UpdateAsync(product);
            TempData["Success"] = "تم تحديث المنتج بنجاح | Product updated successfully";
            return RedirectToAction(nameof(Details), new { id = product.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(product);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            TempData["Success"] = "تم حذف المنتج بنجاح | Product deleted successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> StockReport(int? warehouseId)
    {
        var report = await _stockQuantService.GetStockReportAsync(warehouseId);
        var warehouses = await _warehouseService.GetAllAsync();
        ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");
        ViewBag.CurrentWarehouse = warehouseId;
        return View(report);
    }

    // API endpoint for invoice product search
    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var products = await _productService.GetAllAsync(search: term);
        var result = products.Take(20).Select(p => new
        {
            p.Id,
            p.Name,
            p.NameAr,
            p.InternalReference,
            p.SalePrice,
            p.Cost,
            UomName = p.Uom?.Name,
            CategoryName = p.Category?.Name
        });
        return Json(result);
    }

    private async Task PopulateDropdowns()
    {
        var categories = await _productService.GetCategoriesAsync();
        var warehouses = await _warehouseService.GetAllAsync();
        var locations = await _warehouseService.GetLocationsAsync();

        ViewBag.Categories = new SelectList(categories.Select(c => new { c.Id, Name = c.ParentCategory != null ? $"{c.ParentCategory.Name} / {c.Name}" : c.Name }), "Id", "Name");
        ViewBag.UomList = new SelectList(new[] {
            new { Id = 0, Name = "-- Select --" }
        }, "Id", "Name");

        // We need UoMs - get them through a different path
        ViewBag.ProductTypes = Enum.GetValues<ProductType>().Select(t => new SelectListItem { Value = ((int)t).ToString(), Text = t.ToString() });
        ViewBag.CostMethods = Enum.GetValues<CostMethod>().Select(t => new SelectListItem { Value = ((int)t).ToString(), Text = t.ToString() });
        ViewBag.TrackingTypes = Enum.GetValues<TrackingType>().Select(t => new SelectListItem { Value = ((int)t).ToString(), Text = t.ToString() });
    }
}
