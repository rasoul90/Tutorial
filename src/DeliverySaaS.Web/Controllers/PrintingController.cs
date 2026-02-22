using DeliverySaaS.Web.Models;
using DeliverySaaS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.Web.Controllers;

public class PrintingController : Controller
{
    private readonly IAuthApiClient _apiClient;

    public PrintingController(IAuthApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var settings = await _apiClient.GetBranchPrintSettingsAsync(cancellationToken)
                       ?? new BranchPrintSettingDto("", "Thermal80mm", "Both", 1, false);

        var vm = new BranchPrintSettingsViewModel
        {
            PrinterName = settings.PrinterName,
            PaperSize = settings.PaperSize,
            BarcodeType = settings.BarcodeType,
            Copies = settings.Copies,
            AutoPrint = settings.AutoPrint
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(BranchPrintSettingsViewModel model, CancellationToken cancellationToken)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var ok = await _apiClient.SaveBranchPrintSettingsAsync(
            new BranchPrintSettingDto(model.PrinterName, model.PaperSize, model.BarcodeType, model.Copies, model.AutoPrint),
            cancellationToken);

        TempData[ok ? "Success" : "Error"] = ok ? "تم حفظ إعدادات الطباعة." : "فشل حفظ الإعدادات.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(BranchPrintSettingsViewModel model, CancellationToken cancellationToken)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var paperSize = model.PaperSize switch
        {
            "A4" => 1,
            "A5" => 2,
            "Thermal80mm" => 3,
            "Thermal58mm" => 4,
            _ => 3
        };
        var barcodeType = model.BarcodeType switch
        {
            "Barcode" => 1,
            "Qr" => 2,
            "Both" => 3,
            _ => 3
        };

        var request = new GenerateLabelsRequest(
            paperSize,
            barcodeType,
            model.Copies,
            [new LabelItemDto(
                Barcode: "ORD-2026-0001",
                Qr: "ORD-2026-0001|Branch",
                CompanyLogoBase64: null,
                CompanyPhones: "01000000000 - 01111111111",
                ItemType: "الكترونيات",
                OrderSize: "متوسط",
                Customer: "أحمد محمد",
                MerchantPhone: "01234567890",
                CustomerPhone: "01012345678",
                PriceWithDelivery: 245.50m)]);

        var pdf = await _apiClient.GenerateLabelPreviewAsync(request, cancellationToken);
        if (pdf is null)
        {
            TempData["Error"] = "فشل إنشاء معاينة PDF.";
            return RedirectToAction(nameof(Index));
        }

        return File(pdf, "application/pdf", "label-preview.pdf");
    }

    private bool IsAuthenticated() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("access_token") ?? Request.Cookies["access_token"]);
}
