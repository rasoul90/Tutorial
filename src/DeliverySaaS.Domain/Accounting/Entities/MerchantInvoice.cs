using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class MerchantInvoice : BaseBranchEntity
{
    public Guid MerchantId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime InvoiceDate { get; set; }
}
