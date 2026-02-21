namespace DeliverySaaS.Application.Printing;

public interface ILabelService
{
    byte[] GenerateLabelsPdf(GenerateLabelsRequest request);
}

public record GenerateLabelsRequest(LabelPaperSize PaperSize, List<LabelItemDto> Labels);

public record LabelItemDto(
    string Qr,
    string? CompanyLogoBase64,
    string CompanyPhones,
    string CustomerInfo,
    string MerchantPhone,
    string OrderSize,
    string ItemType,
    decimal Price);

public enum LabelPaperSize
{
    A4 = 1,
    A5 = 2,
    Thermal80mm = 3,
    Thermal58mm = 4
}
