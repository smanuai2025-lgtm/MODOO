using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    public IActionResult Index()
    {
        ViewData["ModuleName"] = "Settings";
        ViewData["ModuleNameAr"] = "الإعدادات";
        ViewData["ModuleIcon"] = "fas fa-cog";
        ViewData["ModuleColor"] = "slate";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }
}
