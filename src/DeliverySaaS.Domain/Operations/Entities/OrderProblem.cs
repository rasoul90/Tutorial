using DeliverySaaS.Domain.Common.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Domain.Operations.Entities;

public class OrderProblem : BaseBranchEntity
{
    public Guid OrderId { get; set; }
    public Guid ProblemCatalogId { get; set; }
    public string? Notes { get; set; }
    public ProblemStatus Status { get; set; } = ProblemStatus.Open;
    public DynamicResolutionType? ResolutionType { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
