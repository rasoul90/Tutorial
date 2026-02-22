using DeliverySaaS.Domain.Accounting.Enums;
using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class MerchantPayment : BaseBranchEntity
{
    public Guid MerchantId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
}
