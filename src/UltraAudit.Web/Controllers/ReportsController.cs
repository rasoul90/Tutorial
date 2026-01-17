using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة التقارير.
/// </summary>
public sealed class ReportsController : Controller
{
    /// <summary>
    /// قوالب التقارير.
    /// </summary>
    public IActionResult Templates() => View();

    /// <summary>
    /// توليد التقارير.
    /// </summary>
    public IActionResult Generate() => View();

    /// <summary>
    /// مركز التصدير.
    /// </summary>
    public IActionResult ExportCenter() => View();
}
