using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Printing.Entities;

public class BranchPrintSetting : BaseBranchEntity
{
    public string PrinterName { get; set; } = string.Empty;
    public string PaperSize { get; set; } = "Thermal80mm";
    public string BarcodeType { get; set; } = "Both";
    public int Copies { get; set; } = 1;
    public bool AutoPrint { get; set; }
}
