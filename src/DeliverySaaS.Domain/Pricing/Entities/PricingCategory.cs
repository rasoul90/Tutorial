using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Pricing.Entities;

public class PricingCategory : BaseBranchEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
