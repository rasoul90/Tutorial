using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class MerchantPaymentAllocation : BaseBranchEntity
{
    public Guid MerchantPaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal AllocatedAmount { get; set; }
}
