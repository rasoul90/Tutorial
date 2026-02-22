using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Identity.Entities;

public class Role : BaseEntity
{
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
