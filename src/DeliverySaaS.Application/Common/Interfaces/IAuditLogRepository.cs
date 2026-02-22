using DeliverySaaS.Domain.Auditing.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<List<AuditLog>> QueryAsync(DateTime from, DateTime to, string? action, string? entityType, int page, int size, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
