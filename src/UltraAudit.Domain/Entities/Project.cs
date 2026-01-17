namespace UltraAudit.Domain.Entities;

/// <summary>
/// يمثل مشروع التدقيق وما يتضمنه من نطاق وفريق عمل.
/// </summary>
public sealed class Project : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<ProjectRisk> Risks { get; set; } = new List<ProjectRisk>();
    public ICollection<ProjectProcedure> Procedures { get; set; } = new List<ProjectProcedure>();
}

/// <summary>
/// عضو ضمن فريق المشروع مع دوره.
/// </summary>
public sealed class ProjectMember : EntityBase
{
    public Guid ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// مخاطرة مرتبطة بمشروع التدقيق.
/// </summary>
public sealed class ProjectRisk : EntityBase
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = "Medium";
}

/// <summary>
/// إجراء تدقيق ضمن مكتبة الإجراءات.
/// </summary>
public sealed class ProjectProcedure : EntityBase
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
