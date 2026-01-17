namespace UltraAudit.Application.Interfaces;

/// <summary>
/// خدمة التحكم في الوصول المبني على الموارد.
/// </summary>
public interface IAclService
{
    /// <summary>
    /// التحقق من صلاحية المستخدم على مورد محدد.
    /// </summary>
    Task<bool> HasPermissionAsync(string userId, string resourceType, string resourceId, string permission, CancellationToken cancellationToken);
}
