using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class Payroll : BaseBranchEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PayrollDate { get; set; }
}
