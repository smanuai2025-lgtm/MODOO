using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        return View();
    }
}
