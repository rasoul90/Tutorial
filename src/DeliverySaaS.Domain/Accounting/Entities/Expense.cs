using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Accounting.Entities;

public class Expense : BaseBranchEntity
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
