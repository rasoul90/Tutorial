using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Printing.Entities;

public class BranchPrintSetting : BaseBranchEntity
{
    public string PrinterName { get; set; } = string.Empty;
    public string PaperSize { get; set; } = "A6";
    public bool AutoPrint { get; set; }
}
