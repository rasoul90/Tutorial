using Microsoft.AspNetCore.Identity;

namespace UltraAudit.Infrastructure.Identity;

/// <summary>
/// مستخدم النظام ضمن هوية ASP.NET Core.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
