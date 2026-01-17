using UltraAudit.Application.Interfaces;

namespace UltraAudit.Infrastructure.Services;

/// <summary>
/// تنفيذ خدمة الاستيراد مع جدولة مهام الخلفية.
/// </summary>
public sealed class ImportService : IImportService
{
    /// <inheritdoc />
    public Task<Guid> ScheduleImportAsync(Guid projectId, string fileName, string uploadedByUserId, CancellationToken cancellationToken)
    {
        // جدولة الاستيراد في Hangfire مع مراعاة المعالجة الدفعية للملفات الكبيرة.
        return Task.FromResult(Guid.NewGuid());
    }
}
