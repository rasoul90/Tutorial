using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Integration.Entities;

public class RoutingRule : BaseBranchEntity
{
    public string RuleName { get; set; } = string.Empty;
    public string ConditionJson { get; set; } = string.Empty;
    public string ActionJson { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
