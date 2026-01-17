using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إدارة الاستثناءات والملاحظات.
/// </summary>
public sealed class ExceptionsController : Controller
{
    /// <summary>
    /// صندوق الاستثناءات.
    /// </summary>
    public IActionResult Inbox() => View();

    /// <summary>
    /// تعيينات الاستثناءات.
    /// </summary>
    public IActionResult Assignments() => View();

    /// <summary>
    /// اتفاقيات مستوى الخدمة وتواريخ الاستحقاق.
    /// </summary>
    public IActionResult Sla() => View();

    /// <summary>
    /// التعليقات والأدلة.
    /// </summary>
    public IActionResult Comments() => View();
}
