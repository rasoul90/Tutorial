using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Pricing.Entities;

public class PricingRate : BaseBranchEntity
{
    public Guid PricingCategoryId { get; set; }
    public Guid AreaId { get; set; }
    public Guid GovernorateId { get; set; }
    public decimal Size1Rate { get; set; }
    public decimal Size2Rate { get; set; }
    public decimal Size3Rate { get; set; }
    public decimal Size4Rate { get; set; }
}
