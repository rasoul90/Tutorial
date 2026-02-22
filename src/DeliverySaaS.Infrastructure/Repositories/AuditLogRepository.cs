using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Auditing.Entities;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public AuditLogRepository(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
        => _dbContext.AuditLogs.AddAsync(log, cancellationToken).AsTask();

    public Task<List<AuditLog>> QueryAsync(DateTime from, DateTime to, string? action, string? entityType, int page, int size, CancellationToken cancellationToken = default)
    {
        var p = page <= 0 ? 1 : page;
        var s = size <= 0 ? 20 : Math.Min(size, 200);
        var q = Scoped(_dbContext.AuditLogs.AsNoTracking()).Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
        if (!string.IsNullOrWhiteSpace(action)) q = q.Where(x => x.Action == action);
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(x => x.EntityType == entityType);
        return q.OrderByDescending(x => x.CreatedAt).Skip((p - 1) * s).Take(s).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<AuditLog> Scoped(IQueryable<AuditLog> q)
    {
        if (_requestContext.IsSaasAdmin) return q;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        q = q.Where(x => x.TenantId == tenantId);
        if (_requestContext.IsCompanyAdmin) return q;
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return q.Where(x => x.BranchId == branchId);
    }
}
