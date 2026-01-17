namespace UltraAudit.Domain.Entities;

/// <summary>
/// يمثل حالة استثناء (ملاحظة) ضمن سير العمل.
/// </summary>
public sealed class ExceptionCase : EntityBase
{
    public Guid ProjectId { get; set; }
    public Guid AnalysisRunId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public DateTime? DueDateUtc { get; set; }
    public string? AssignedToUserId { get; set; }
    public ICollection<ExceptionComment> Comments { get; set; } = new List<ExceptionComment>();
}

/// <summary>
/// تعليق على حالة استثناء.
/// </summary>
public sealed class ExceptionComment : EntityBase
{
    public Guid ExceptionCaseId { get; set; }
    public string AuthorUserId { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}
