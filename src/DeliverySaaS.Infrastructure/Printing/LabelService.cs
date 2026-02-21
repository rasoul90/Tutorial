using DeliverySaaS.Application.Printing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DeliverySaaS.Infrastructure.Printing;

public class LabelService : ILabelService
{
    public LabelService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateLabelsPdf(GenerateLabelsRequest request)
    {
        var document = Document.Create(container =>
        {
            foreach (var label in request.Labels)
            {
                container.Page(page =>
                {
                    ConfigurePageSize(page, request.PaperSize);
                    page.Margin(10);

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);

                        if (!string.IsNullOrWhiteSpace(label.CompanyLogoBase64))
                        {
                            var logoBytes = Convert.FromBase64String(label.CompanyLogoBase64);
                            column.Item().AlignCenter().Height(40).Image(logoBytes);
                        }

                        column.Item().AlignCenter().Text($"Company Phones: {label.CompanyPhones}").FontSize(10).SemiBold();

                        column.Item().Border(1).Padding(6).Column(info =>
                        {
                            info.Spacing(4);
                            info.Item().Text($"Customer Info: {label.CustomerInfo}");
                            info.Item().Text($"Merchant Phone: {label.MerchantPhone}");
                            info.Item().Text($"Order Size: {label.OrderSize}");
                            info.Item().Text($"Item Type: {label.ItemType}");
                            info.Item().Text($"Price: {label.Price:0.00}");
                        });

                        column.Item().AlignCenter().Text("QR").Bold();
                        column.Item().AlignCenter().QrCode(label.Qr);
                    });
                });
            }
        });

        return document.GeneratePdf();
    }

    private static void ConfigurePageSize(PageDescriptor page, LabelPaperSize paperSize)
    {
        switch (paperSize)
        {
            case LabelPaperSize.A4:
                page.Size(PageSizes.A4);
                break;
            case LabelPaperSize.A5:
                page.Size(PageSizes.A5);
                break;
            case LabelPaperSize.Thermal80mm:
                page.Size(80, 150, Unit.Millimetre);
                break;
            case LabelPaperSize.Thermal58mm:
                page.Size(58, 120, Unit.Millimetre);
                break;
            default:
                throw new InvalidOperationException("Unsupported label paper size.");
        }
    }
}
