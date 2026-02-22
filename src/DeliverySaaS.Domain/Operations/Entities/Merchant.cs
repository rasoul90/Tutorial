using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Operations.Entities;

public class Merchant : BaseBranchEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? PricingCategoryId { get; set; }
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
