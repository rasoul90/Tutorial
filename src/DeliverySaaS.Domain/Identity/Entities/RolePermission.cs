using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Identity.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
