using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة مشاريع التدقيق.
/// </summary>
public sealed class ProjectsController : Controller
{
    /// <summary>
    /// قائمة المشاريع.
    /// </summary>
    public IActionResult Index() => View();

    /// <summary>
    /// إنشاء مشروع جديد.
    /// </summary>
    public IActionResult Create() => View();

    /// <summary>
    /// إدارة الفرق والأعضاء.
    /// </summary>
    public IActionResult Teams() => View();

    /// <summary>
    /// مكتبة المخاطر والإجراءات.
    /// </summary>
    public IActionResult Risks() => View();
}
