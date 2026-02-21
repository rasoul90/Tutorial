using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Operations.Entities;

public class OrderEvent : BaseBranchEntity
{
    public Guid OrderId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime EventAt { get; set; }
}
