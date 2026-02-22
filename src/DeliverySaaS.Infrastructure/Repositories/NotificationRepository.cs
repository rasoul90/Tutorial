using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Notifications.Entities;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public NotificationRepository(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        => _dbContext.Notifications.AddAsync(notification, cancellationToken).AsTask();

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.Notifications).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<Notification>> GetBranchNotificationsAsync(bool unreadOnly, int page, int size, CancellationToken cancellationToken = default)
    {
        var p = page <= 0 ? 1 : page;
        var s = size <= 0 ? 20 : Math.Min(size, 200);
        var query = Scoped(_dbContext.Notifications.AsNoTracking()).OrderByDescending(x => x.CreatedAt);
        if (unreadOnly) query = query.Where(x => !x.IsRead).OrderByDescending(x => x.CreatedAt);
        return query.Skip((p - 1) * s).Take(s).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Notification> Scoped(IQueryable<Notification> q)
    {
        if (_requestContext.IsSaasAdmin) return q;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        q = q.Where(x => x.TenantId == tenantId);
        if (_requestContext.IsCompanyAdmin) return q;
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return q.Where(x => x.BranchId == branchId);
    }
}
