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
        var copies = Math.Max(1, request.Copies);

        var document = Document.Create(container =>
        {
            foreach (var label in request.Labels)
            {
                for (var i = 0; i < copies; i++)
                {
                    container.Page(page =>
                    {
                        ConfigurePageSize(page, request.PaperSize);
                        page.Margin(8);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Content().Column(column =>
                        {
                            column.Spacing(6);

                            if (!string.IsNullOrWhiteSpace(label.CompanyLogoBase64))
                            {
                                try
                                {
                                    var logoBytes = Convert.FromBase64String(label.CompanyLogoBase64);
                                    column.Item().AlignCenter().Height(35).Image(logoBytes, ImageScaling.FitHeight);
                                }
                                catch
                                {
                                    column.Item().AlignCenter().Text("Logo").SemiBold();
                                }
                            }

                            column.Item().AlignCenter().Text($"Phones: {label.CompanyPhones}").SemiBold();

                            column.Item().Border(1).Padding(5).Column(info =>
                            {
                                info.Spacing(2);
                                info.Item().Text($"Item Type: {label.ItemType}");
                                info.Item().Text($"Size: {label.OrderSize}");
                                info.Item().Text($"Customer: {label.Customer}");
                                info.Item().Text($"Merchant Phone: {label.MerchantPhone}");
                                info.Item().Text($"Customer Phone: {label.CustomerPhone}");
                                info.Item().Text($"Price With Delivery: {label.PriceWithDelivery:0.00}").Bold();
                            });

                            if (request.BarcodeType is BarcodeType.Barcode or BarcodeType.Both)
                            {
                                column.Item().Border(1).Padding(4).AlignCenter().Text($"Barcode: {label.Barcode}").FontSize(9);
                            }

                            if (request.BarcodeType is BarcodeType.Qr or BarcodeType.Both)
                            {
                                column.Item().Border(1).Padding(4).AlignCenter().Text($"QR: {label.Qr}").FontSize(9);
                            }
                        });
                    });
                }
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
