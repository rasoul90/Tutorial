using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Geo.Entities;

public class Governorate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
