using UltraAudit.Application.DTOs;

namespace UltraAudit.Application.Interfaces;

/// <summary>
/// خدمة تنفيذ أوامر التحليل على مجموعات البيانات.
/// </summary>
public interface ICommandExecutionService
{
    /// <summary>
    /// تنفيذ الأمر وإرجاع ملخص النتائج.
    /// </summary>
    Task<RunCommandResultDto> ExecuteAsync(RunCommandRequestDto request, string initiatedByUserId, CancellationToken cancellationToken);
}
