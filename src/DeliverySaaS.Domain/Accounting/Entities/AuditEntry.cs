using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class AuditEntry : BaseBranchEntity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}
