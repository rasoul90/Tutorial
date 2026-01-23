using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rasad.Infrastructure.Identity;
using Microsoft.Extensions.Localization;
using Rasad.Web.Models;
using Rasad.Web.Resources;

namespace Rasad.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AccountController(SignInManager<ApplicationUser> signInManager, IStringLocalizer<SharedResources> localizer)
    {
        _signInManager = signInManager;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, _localizer["InvalidLogin"]);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}
