using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UltraAudit.Application.DTOs;
using UltraAudit.Application.Interfaces;
using UltraAudit.Domain.Entities;
using UltraAudit.Infrastructure.Data;

namespace UltraAudit.Infrastructure.Services;

/// <summary>
/// تنفيذ أوامر التحليل مع إنشاء سجل تشغيل ثابت.
/// </summary>
public sealed class CommandExecutionService : ICommandExecutionService
{
    private readonly UasDbContext _dbContext;
    private readonly IAuditTrailService _auditTrailService;

    public CommandExecutionService(UasDbContext dbContext, IAuditTrailService auditTrailService)
    {
        _dbContext = dbContext;
        _auditTrailService = auditTrailService;
    }

    /// <inheritdoc />
    public async Task<RunCommandResultDto> ExecuteAsync(RunCommandRequestDto request, string initiatedByUserId, CancellationToken cancellationToken)
    {
        // تنفيذ الأمر التحليلي وفق الكتالوج (Placeholder) مع حفظ لقطة المعلمات.
        var analysisRun = new AnalysisRun
        {
            DatasetId = request.DatasetId,
            AnalysisId = Guid.NewGuid(),
            Status = "Completed",
            ParametersSnapshotJson = request.ParametersJson,
            ResultDataset = $"results_{Guid.NewGuid():N}",
            Summary = $"تم تنفيذ الأمر {request.CommandName} بنجاح.",
        };

        // إنشاء بصمة تكامل لضمان ثبات بيانات التشغيل.
        analysisRun.IntegrityHash = ComputeIntegrityHash(analysisRun);
        _dbContext.AnalysisRuns.Add(analysisRun);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditTrailService.RecordAsync(
            "RunCommand",
            nameof(AnalysisRun),
            analysisRun.Id.ToString(),
            initiatedByUserId,
            request.ParametersJson,
            cancellationToken);

        return new RunCommandResultDto
        {
            AnalysisRunId = analysisRun.Id,
            Status = analysisRun.Status,
            Summary = analysisRun.Summary,
        };
    }

    private static string ComputeIntegrityHash(AnalysisRun run)
    {
        var payload = $"{run.AnalysisId}|{run.DatasetId}|{run.ParametersSnapshotJson}|{run.ResultDataset}|{run.CreatedAtUtc:o}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
