using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Integration.Entities;

public class InboxMessage : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool IsProcessed { get; set; }
}
