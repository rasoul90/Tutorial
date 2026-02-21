using DeliverySaaS.Domain.Common.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Domain.Operations.Entities;

public class Order : BaseBranchEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid MerchantId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal AmountToCollect { get; set; }
    public OperationalState State { get; set; } = OperationalState.New;
}
