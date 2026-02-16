using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return RedirectToAction("Login", "AccountManagement");
    }

    public IActionResult Error(int? code)
    {
        ViewData["Title"] = "Error";
        return View();
    }
}
