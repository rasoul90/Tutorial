using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Common.Models;
using DeliverySaaS.Domain.Auditing.Entities;

namespace DeliverySaaS.Application.Auditing;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly IRequestContext _requestContext;

    public AuditLogService(IAuditLogRepository repository, IRequestContext requestContext)
    {
        _repository = repository;
        _requestContext = requestContext;
    }

    public async Task WriteAsync(string action, string entityType, string entityId, string summaryAr, string diffJson, string? ip = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            BranchId = _requestContext.BranchId,
            TenantId = _requestContext.TenantId ?? Guid.Empty,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            SummaryAr = summaryAr,
            DiffJson = diffJson,
            Ip = ip,
            UserAgent = userAgent
        };

        await _repository.AddAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> QueryAsync(DateTime from, DateTime to, string? action, string? entityType, int page, int size, CancellationToken cancellationToken = default)
    {
        var rows = await _repository.QueryAsync(from, to, action, entityType, page, size, cancellationToken);
        return new PagedResult<AuditLogDto>(rows.Select(x => new AuditLogDto(x.Id, x.Action, x.EntityType, x.EntityId, x.SummaryAr, x.DiffJson, x.CreatedAt)).ToList(), page, size);
    }
}
