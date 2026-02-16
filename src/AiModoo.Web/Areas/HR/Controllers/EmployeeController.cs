using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Areas.HR.Controllers;

[Area("HR")]
[Authorize]
public class EmployeeController : Controller
{
    public IActionResult Index()
    {
        ViewData["ModuleName"] = "Human Resources";
        ViewData["ModuleNameAr"] = "الموارد البشرية";
        ViewData["ModuleIcon"] = "fas fa-users";
        ViewData["ModuleColor"] = "pink";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }
}
