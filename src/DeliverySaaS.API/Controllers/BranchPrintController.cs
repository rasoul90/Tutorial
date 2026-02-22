using System.Text.Json;
using DeliverySaaS.API.Security;
using DeliverySaaS.Application.Printing;
using DeliverySaaS.Domain.Printing.Entities;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/branch/print")]
[Authorize(Roles = "BranchManager")]
[Authorize(Policy = "BranchScope")]
public class BranchPrintController : ControllerBase
{
    private readonly ILabelService _labelService;
    private readonly ApplicationDbContext _dbContext;
    private readonly RequestContext _requestContext;

    public BranchPrintController(ILabelService labelService, ApplicationDbContext dbContext, RequestContext requestContext)
    {
        _labelService = labelService;
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    [HttpGet("settings")]
    public async Task<ActionResult<BranchPrintSettingDto>> GetSettings(CancellationToken cancellationToken)
    {
        var branchId = RequiredBranchId();
        var tenantId = RequiredTenantId();

        var setting = await _dbContext.BranchPrintSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.BranchId == branchId, cancellationToken);

        if (setting is null)
        {
            return Ok(new BranchPrintSettingDto("", "Thermal80mm", "Both", 1, false));
        }

        return Ok(new BranchPrintSettingDto(setting.PrinterName, setting.PaperSize, setting.BarcodeType, setting.Copies, setting.AutoPrint));
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpsertSettings([FromBody] BranchPrintSettingDto request, CancellationToken cancellationToken)
    {
        var branchId = RequiredBranchId();
        var tenantId = RequiredTenantId();

        var setting = await _dbContext.BranchPrintSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.BranchId == branchId, cancellationToken);

        if (setting is null)
        {
            setting = new BranchPrintSetting
            {
                TenantId = tenantId,
                BranchId = branchId,
                PrinterName = request.PrinterName,
                PaperSize = request.PaperSize,
                BarcodeType = request.BarcodeType,
                Copies = request.Copies,
                AutoPrint = request.AutoPrint
            };
            _dbContext.BranchPrintSettings.Add(setting);
        }
        else
        {
            setting.PrinterName = request.PrinterName;
            setting.PaperSize = request.PaperSize;
            setting.BarcodeType = request.BarcodeType;
            setting.Copies = request.Copies;
            setting.AutoPrint = request.AutoPrint;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("settings")]
    public async Task<IActionResult> DeleteSettings(CancellationToken cancellationToken)
    {
        var branchId = RequiredBranchId();
        var tenantId = RequiredTenantId();

        var setting = await _dbContext.BranchPrintSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.BranchId == branchId, cancellationToken);

        if (setting is null)
        {
            return NoContent();
        }

        _dbContext.BranchPrintSettings.Remove(setting);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("labels")]
    public async Task<IActionResult> PrintLabels([FromBody] GenerateLabelsRequest request, CancellationToken cancellationToken)
    {
        var branchId = RequiredBranchId();
        var tenantId = RequiredTenantId();
        var pdf = _labelService.GenerateLabelsPdf(request);

        var jobId = Guid.NewGuid();
        _dbContext.PrintJobs.Add(new PrintJob
        {
            Id = jobId,
            TenantId = tenantId,
            BranchId = branchId,
            JobType = "LABEL",
            Status = "Generated",
            RequestedAt = DateTime.UtcNow,
            FileName = $"labels-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
        });

        foreach (var item in request.Labels)
        {
            _dbContext.PrintJobItems.Add(new PrintJobItem
            {
                TenantId = tenantId,
                BranchId = branchId,
                PrintJobId = jobId,
                ReferenceId = Guid.NewGuid(),
                ReferenceType = "Label",
                PayloadJson = JsonSerializer.Serialize(item)
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return File(pdf, "application/pdf", "labels-preview.pdf");
    }

    private Guid RequiredTenantId() => _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
    private Guid RequiredBranchId() => _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
}

public sealed record BranchPrintSettingDto(
    string PrinterName,
    string PaperSize,
    string BarcodeType,
    int Copies,
    bool AutoPrint);
