using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة الدخول وتبديل اللغة.
/// </summary>
public sealed class AccountController : Controller
{
    /// <summary>
    /// صفحة تسجيل الدخول.
    /// </summary>
    public IActionResult Login() => View();

    /// <summary>
    /// تبديل اللغة.
    /// </summary>
    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            ".AspNetCore.Culture",
            $"c={culture}|uic={culture}");

        return LocalRedirect(returnUrl);
    }
}
