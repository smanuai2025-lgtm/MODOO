using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Areas.AI.Controllers;

[Area("AI")]
[Authorize]
public class DashboardAiController : Controller
{
    public IActionResult Index()
    {
        ViewData["ModuleName"] = "AI Assistant";
        ViewData["ModuleNameAr"] = "الذكاء الاصطناعي";
        ViewData["ModuleIcon"] = "fas fa-brain";
        ViewData["ModuleColor"] = "yellow";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }
}
