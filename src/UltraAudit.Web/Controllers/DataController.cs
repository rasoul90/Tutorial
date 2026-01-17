using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة البيانات والاستيراد.
/// </summary>
public sealed class DataController : Controller
{
    /// <summary>
    /// معالج الاستيراد.
    /// </summary>
    public IActionResult ImportWizard() => View();

    /// <summary>
    /// استعراض مجموعات البيانات.
    /// </summary>
    public IActionResult Datasets() => View();

    /// <summary>
    /// قاموس البيانات.
    /// </summary>
    public IActionResult Dictionary() => View();

    /// <summary>
    /// جودة البيانات.
    /// </summary>
    public IActionResult Quality() => View();
}
