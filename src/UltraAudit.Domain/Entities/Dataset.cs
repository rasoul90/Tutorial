namespace UltraAudit.Domain.Entities;

/// <summary>
/// يمثل مجموعة بيانات مستوردة إلى النظام.
/// </summary>
public sealed class Dataset : EntityBase
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string StorageTable { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public ICollection<DatasetField> Fields { get; set; } = new List<DatasetField>();
}

/// <summary>
/// حقل ضمن قاموس بيانات مجموعة البيانات.
/// </summary>
public sealed class DatasetField : EntityBase
{
    public Guid DatasetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? ValidationRule { get; set; }
}
