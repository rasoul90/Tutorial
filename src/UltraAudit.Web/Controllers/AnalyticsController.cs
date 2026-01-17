using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة التحليلات والأوامر.
/// </summary>
public sealed class AnalyticsController : Controller
{
    /// <summary>
    /// معالج بناء التحليل.
    /// </summary>
    public IActionResult Builder() => View();

    /// <summary>
    /// مكتبة التحليلات.
    /// </summary>
    public IActionResult Library() => View();

    /// <summary>
    /// تشغيل التحليلات.
    /// </summary>
    public IActionResult Runs() => View();

    /// <summary>
    /// مستكشف النتائج.
    /// </summary>
    public IActionResult Results() => View();

    /// <summary>
    /// السكربتات والأتمتة.
    /// </summary>
    public IActionResult Automation() => View();
}
