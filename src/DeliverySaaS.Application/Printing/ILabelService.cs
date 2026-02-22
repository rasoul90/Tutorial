namespace DeliverySaaS.Application.Printing;

public interface ILabelService
{
    byte[] GenerateLabelsPdf(GenerateLabelsRequest request);
}

public record GenerateLabelsRequest(
    LabelPaperSize PaperSize,
    BarcodeType BarcodeType,
    int Copies,
    List<LabelItemDto> Labels);

public record LabelItemDto(
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

public enum LabelPaperSize
{
    A4 = 1,
    A5 = 2,
    Thermal80mm = 3,
    Thermal58mm = 4
}

public enum BarcodeType
{
    Barcode = 1,
    Qr = 2,
    Both = 3
}
