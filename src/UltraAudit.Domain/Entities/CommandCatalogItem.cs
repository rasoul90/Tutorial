namespace UltraAudit.Domain.Entities;

/// <summary>
/// عنصر في كتالوج أوامر التحليل مع مخطط المعلمات.
/// </summary>
public sealed class CommandCatalogItem : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParameterSchemaJson { get; set; } = "{}";
}
