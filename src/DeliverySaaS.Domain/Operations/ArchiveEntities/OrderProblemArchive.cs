using DeliverySaaS.Domain.Common.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Domain.Operations.ArchiveEntities;

public class OrderProblemArchive : BaseBranchEntity
{
    public Guid OriginalOrderProblemId { get; set; }
    public Guid OrderId { get; set; }
    public ProblemStatus Status { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime ArchivedAt { get; set; }
}
