namespace UltraAudit.Application.Interfaces;

/// <summary>
/// خدمة استيراد البيانات مع المعالجة الدفعية.
/// </summary>
public interface IImportService
{
    /// <summary>
    /// جدولة عملية استيراد عبر المعالج الخلفي.
    /// </summary>
    Task<Guid> ScheduleImportAsync(Guid projectId, string fileName, string uploadedByUserId, CancellationToken cancellationToken);
}
