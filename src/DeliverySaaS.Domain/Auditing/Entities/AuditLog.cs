using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Auditing.Entities;

public class AuditLog : BaseEntity
{
    public Guid? BranchId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string SummaryAr { get; set; } = string.Empty;
    public string DiffJson { get; set; } = "{}";
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
}
