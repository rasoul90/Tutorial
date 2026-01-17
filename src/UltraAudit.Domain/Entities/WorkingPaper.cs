namespace UltraAudit.Domain.Entities;

/// <summary>
/// ورقة عمل مرتبطة بإجراء أو مخاطرة.
/// </summary>
public sealed class WorkingPaper : EntityBase
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string PreparedByUserId { get; set; } = string.Empty;
    public string? ReviewedByUserId { get; set; }
    public string? Content { get; set; }
}
