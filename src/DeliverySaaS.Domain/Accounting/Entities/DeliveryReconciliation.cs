using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class DeliveryReconciliation : BaseBranchEntity
{
    public Guid DeliveryAgentId { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal DeliveredAmount { get; set; }
    public DateTime ReconciledAt { get; set; }
}
