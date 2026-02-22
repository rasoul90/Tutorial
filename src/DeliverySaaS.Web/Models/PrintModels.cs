using System.ComponentModel.DataAnnotations;

namespace DeliverySaaS.Web.Models;

public class BranchPrintSettingsViewModel
{
    [Display(Name = "اسم الطابعة")]
    public string PrinterName { get; set; } = string.Empty;

    [Display(Name = "نوع الورق")]
    public string PaperSize { get; set; } = "Thermal80mm";

    [Display(Name = "نوع الباركود")]
    public string BarcodeType { get; set; } = "Both";

    [Range(1, 20)]
    [Display(Name = "عدد الوصولات")]
    public int Copies { get; set; } = 1;

    [Display(Name = "طباعة تلقائية")]
    public bool AutoPrint { get; set; }
}

public sealed record BranchPrintSettingDto(string PrinterName, string PaperSize, string BarcodeType, int Copies, bool AutoPrint);

public sealed record GenerateLabelsRequest(int PaperSize, int BarcodeType, int Copies, List<LabelItemDto> Labels);

public sealed record LabelItemDto(
    string Barcode,
    string Qr,
    string? CompanyLogoBase64,
    string CompanyPhones,
    string ItemType,
    string OrderSize,
    string Customer,
    string MerchantPhone,
    string CustomerPhone,
    decimal PriceWithDelivery);
