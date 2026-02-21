using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Operations.Entities;

public class PickupAgent : BaseBranchEntity
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
