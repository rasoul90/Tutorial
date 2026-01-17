namespace UltraAudit.Domain.Entities;

/// <summary>
/// صلاحية مخصصة لمورد محدد ضمن قائمة التحكم في الوصول.
/// </summary>
public sealed class ResourceGrant : EntityBase
{
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string PrincipalId { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
}
