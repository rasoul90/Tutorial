namespace UltraAudit.Application.DTOs;

/// <summary>
/// نموذج أمر في الكتالوج لواجهة المستخدم.
/// </summary>
public sealed class CommandCatalogDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParameterSchemaJson { get; set; } = "{}";
}

/// <summary>
/// طلب تنفيذ أمر تحليلي على مجموعة بيانات.
/// </summary>
public sealed class RunCommandRequestDto
{
    public Guid DatasetId { get; set; }
    public string CommandName { get; set; } = string.Empty;
    public string ParametersJson { get; set; } = "{}";
}

/// <summary>
/// استجابة تنفيذ أمر تتضمن ملخص النتائج.
/// </summary>
public sealed class RunCommandResultDto
{
    public Guid AnalysisRunId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
