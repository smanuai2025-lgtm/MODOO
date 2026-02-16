using AiModoo.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AiModoo.Web.Controllers;

public class AccountManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountManagementController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "Login";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError(string.Empty, "Email and password are required.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
            return View();
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewData["Title"] = "Register";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string fullNameAr, string email, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            FullNameAr = fullNameAr,
            PreferredLanguage = "ar",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Core.Constants.Roles.Employee);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Dashboard");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View("Error");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        ViewData["Title"] = "Profile";
        return View(user);
    }
}
