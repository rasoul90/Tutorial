using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class MerchantSettlementRequest : BaseBranchEntity
{
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
}
