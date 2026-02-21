using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Printing.Entities;

public class PrintJob : BaseBranchEntity
{
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = "Queued";
    public DateTime RequestedAt { get; set; }
}
