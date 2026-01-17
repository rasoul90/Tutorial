namespace UltraAudit.Domain.Entities;

/// <summary>
/// تعريف تحليل قابل لإعادة التشغيل مع الأوامر المترابطة.
/// </summary>
public sealed class Analysis : EntityBase
{
    public Guid ProjectId { get; set; }
    public Guid DatasetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<AnalysisCommand> Commands { get; set; } = new List<AnalysisCommand>();
}

/// <summary>
/// أمر تحليلي ضمن تحليل.
/// </summary>
public sealed class AnalysisCommand : EntityBase
{
    public Guid AnalysisId { get; set; }
    public string CommandName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ParametersJson { get; set; } = "{}";
    public int Order { get; set; }
}

/// <summary>
/// نتيجة تشغيل تحليل مع بيانات ثابتة للمعلمات.
/// </summary>
public sealed class AnalysisRun : EntityBase
{
    public Guid AnalysisId { get; set; }
    public Guid DatasetId { get; set; }
    public string Status { get; set; } = "Pending";
    public string ParametersSnapshotJson { get; set; } = "{}";
    public string ResultDataset { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string IntegrityHash { get; set; } = string.Empty;
}
