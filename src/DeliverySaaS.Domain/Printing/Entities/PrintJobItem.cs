using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Printing.Entities;

public class PrintJobItem : BaseBranchEntity
{
    public Guid PrintJobId { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
}
