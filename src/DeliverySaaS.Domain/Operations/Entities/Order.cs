using DeliverySaaS.Domain.Common.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Domain.Operations.Entities;

public class Order : BaseBranchEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid MerchantId { get; set; }
    public Guid? DeliveryAgentId { get; set; }
    public Guid? GovernorateId { get; set; }
    public Guid? PricingCategoryId { get; set; }
    public OrderSize? OrderSize { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? InternalNote { get; set; }
    public decimal AmountToCollect { get; set; }
    public OperationalState State { get; set; } = OperationalState.New;
    public bool HasProblem { get; set; }
    public ProblemStatus ProblemStatus { get; set; } = ProblemStatus.None;

    public DateTime? DeliveredAt { get; set; }
    public decimal? DeliveredPriceWithDelivery { get; set; }
    public decimal? DeliveryFeeApplied { get; set; }
    public decimal? DeliveryAgentFeeApplied { get; set; }
    public decimal? CompanyNetDeliveryProfit { get; set; }
    public decimal? MerchantDueAmount { get; set; }
    public bool IsDeliveryAgentSettled { get; set; }
    public DateTime? DeliveryAgentSettledAt { get; set; }
    public bool IsMerchantSettled { get; set; }
    public DateTime? MerchantSettledAt { get; set; }
    public bool HasReturn { get; set; }
    public DateTime? ReturnInitiatedAt { get; set; }
}
