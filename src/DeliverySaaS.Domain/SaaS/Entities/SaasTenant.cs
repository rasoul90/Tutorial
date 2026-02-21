using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.SaaS.Entities;

public class SaasTenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
