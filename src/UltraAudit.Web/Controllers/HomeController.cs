using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// تحكم رئيسي لصفحات لوحة التحكم.
/// </summary>
public sealed class HomeController : Controller
{
    /// <summary>
    /// الصفحة الرئيسية للوحة التحكم.
    /// </summary>
    public IActionResult Index() => View();

    /// <summary>
    /// مهامي الحالية.
    /// </summary>
    public IActionResult MyTasks() => View();

    /// <summary>
    /// النشاط الأخير.
    /// </summary>
    public IActionResult RecentActivity() => View();
}
