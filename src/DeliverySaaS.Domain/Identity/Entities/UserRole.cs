using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Identity.Entities;

public class UserRole : BaseEntity
{
    public Guid? BranchId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
