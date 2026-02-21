using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Operations.Entities;

public class OrderProblem : BaseBranchEntity
{
    public Guid OrderId { get; set; }
    public string ProblemType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
