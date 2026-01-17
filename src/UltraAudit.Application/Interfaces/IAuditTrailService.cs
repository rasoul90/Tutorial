namespace UltraAudit.Application.Interfaces;

/// <summary>
/// خدمة تسجيل العمليات في سجل التدقيق.
/// </summary>
public interface IAuditTrailService
{
    /// <summary>
    /// تسجيل عملية مع توليد بصمة تكامل.
    /// </summary>
    Task RecordAsync(string action, string entityName, string entityId, string userId, string metadataJson, CancellationToken cancellationToken);
}
