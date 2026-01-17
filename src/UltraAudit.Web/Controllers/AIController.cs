using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة رؤى الذكاء الاصطناعي.
/// </summary>
public sealed class AIController : Controller
{
    /// <summary>
    /// رؤى مجموعة البيانات.
    /// </summary>
    public IActionResult DatasetInsights() => View();

    /// <summary>
    /// رؤى تشغيل التحليل.
    /// </summary>
    public IActionResult RunInsights() => View();

    /// <summary>
    /// تقييم المخاطر.
    /// </summary>
    public IActionResult RiskScoring() => View();

    /// <summary>
    /// مستكشف الشذوذ.
    /// </summary>
    public IActionResult AnomalyExplorer() => View();

    /// <summary>
    /// المطابقة التقريبية.
    /// </summary>
    public IActionResult Similarity() => View();
}
