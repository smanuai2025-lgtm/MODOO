using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AiModoo.Web.Areas.Sales.Controllers;

[Area("Sales")]
[Authorize]
public class PricelistController : Controller
{
    private readonly IPricelistService _pricelistService;
    private readonly IProductService _productService;

    public PricelistController(
        IPricelistService pricelistService,
        IProductService productService)
    {
        _pricelistService = pricelistService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var pricelists = await _pricelistService.GetAllAsync();
        return View(pricelists);
    }

    public async Task<IActionResult> Details(int id)
    {
        var pricelist = await _pricelistService.GetByIdAsync(id);
        if (pricelist == null) return NotFound();
        return View(pricelist);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Pricelist { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pricelist pricelist)
    {
        try
        {
            var created = await _pricelistService.CreateAsync(pricelist);
            TempData["Success"] = "تم إنشاء قائمة الأسعار بنجاح | Pricelist created successfully";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateDropdowns();
            return View(pricelist);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _pricelistService.DeleteAsync(id);
            TempData["Success"] = "تم حذف قائمة الأسعار | Pricelist deleted";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns()
    {
        var products = await _productService.GetAllAsync();
        ViewBag.Products = new SelectList(products.Where(p => p.CanBeSold), "Id", "Name");

        var categories = await _productService.GetCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
    }
}
