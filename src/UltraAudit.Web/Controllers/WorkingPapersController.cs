using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة أوراق العمل.
/// </summary>
public sealed class WorkingPapersController : Controller
{
    /// <summary>
    /// مكتبة أوراق العمل.
    /// </summary>
    public IActionResult Library() => View();

    /// <summary>
    /// المراجعة والاعتمادات.
    /// </summary>
    public IActionResult Reviews() => View();

    /// <summary>
    /// مستودع الأدلة.
    /// </summary>
    public IActionResult Evidence() => View();
}
