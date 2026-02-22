using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Operations.ArchiveEntities;

public class OrderEventArchive : BaseBranchEntity
{
    public Guid OriginalOrderEventId { get; set; }
    public Guid OrderId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime EventAt { get; set; }
    public DateTime ArchivedAt { get; set; }
}
