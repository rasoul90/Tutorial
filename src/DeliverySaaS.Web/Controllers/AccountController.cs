using DeliverySaaS.Web.Models;
using DeliverySaaS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IConfiguration _configuration;

    public AccountController(IAuthApiClient authApiClient, IConfiguration configuration)
    {
        _authApiClient = authApiClient;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("access_token")))
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tenantIdRaw = _configuration["Auth:TenantId"];
        if (!Guid.TryParse(tenantIdRaw, out var tenantId))
        {
            ModelState.AddModelError(string.Empty, "TenantId غير صحيح في الإعدادات.");
            return View(model);
        }

        var result = await _authApiClient.LoginAsync(new LoginRequest(model.UserName, model.Password, tenantId), cancellationToken);
        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "فشل تسجيل الدخول. تحقق من البيانات.");
            return View(model);
        }

        HttpContext.Session.SetString("access_token", result.AccessToken);
        HttpContext.Session.SetString("refresh_token", result.RefreshToken);

        Response.Cookies.Append("access_token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = result.ExpiresAtUtc
        });

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("access_token");
        return RedirectToAction(nameof(Login));
    }
}
