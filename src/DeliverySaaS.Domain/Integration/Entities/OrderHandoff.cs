using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Integration.Entities;

public class OrderHandoff : BaseBranchEntity
{
    public Guid OrderId { get; set; }
    public Guid PartnerConnectionId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? SentAt { get; set; }
}
