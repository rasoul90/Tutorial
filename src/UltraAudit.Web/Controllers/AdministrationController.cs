using Microsoft.AspNetCore.Mvc;

namespace UltraAudit.Web.Controllers;

/// <summary>
/// إعدادات الإدارة والصلاحيات.
/// </summary>
public sealed class AdministrationController : Controller
{
    /// <summary>
    /// إدارة المستخدمين والأدوار.
    /// </summary>
    public IActionResult UsersRoles() => View();

    /// <summary>
    /// كتالوج الصلاحيات.
    /// </summary>
    public IActionResult Permissions() => View();

    /// <summary>
    /// مدير قوائم التحكم بالوصول.
    /// </summary>
    public IActionResult AclManager() => View();

    /// <summary>
    /// سجل التدقيق.
    /// </summary>
    public IActionResult AuditTrail() => View();

    /// <summary>
    /// إعدادات النظام.
    /// </summary>
    public IActionResult Settings() => View();

    /// <summary>
    /// مراقبة المهام الخلفية.
    /// </summary>
    public IActionResult Jobs() => View();
}
