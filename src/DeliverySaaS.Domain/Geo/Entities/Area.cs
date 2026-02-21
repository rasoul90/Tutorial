using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Geo.Entities;

public class Area : BaseEntity
{
    public Guid GovernorateId { get; set; }
    public string Name { get; set; } = string.Empty;
}
