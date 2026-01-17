namespace UltraAudit.Domain.Entities;

/// <summary>
/// سجل تدقيق للعمليات الحساسة.
/// </summary>
public sealed class AuditTrailEntry : EntityBase
{
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string PerformedByUserId { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
    public string IntegrityHash { get; set; } = string.Empty;
}
