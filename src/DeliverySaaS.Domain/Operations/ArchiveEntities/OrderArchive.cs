using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Operations.ArchiveEntities;

public class OrderArchive : BaseBranchEntity
{
    public Guid OriginalOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid MerchantId { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public decimal? MerchantDueAmount { get; set; }
    public bool IsMerchantSettled { get; set; }
    public bool HasReturn { get; set; }
    public DateTime ArchivedAt { get; set; }
}
