using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Identity.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
