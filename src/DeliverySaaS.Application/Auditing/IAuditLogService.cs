using DeliverySaaS.Application.Common.Models;

namespace DeliverySaaS.Application.Auditing;

public interface IAuditLogService
{
    Task WriteAsync(string action, string entityType, string entityId, string summaryAr, string diffJson, string? ip = null, string? userAgent = null, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> QueryAsync(DateTime from, DateTime to, string? action, string? entityType, int page, int size, CancellationToken cancellationToken = default);
}
