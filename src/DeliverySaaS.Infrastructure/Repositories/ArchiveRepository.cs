using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.ArchiveEntities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class ArchiveRepository : IArchiveRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public ArchiveRepository(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public Task<List<Order>> GetOrdersEligibleForArchiveAsync(DateTime cutoff, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.Orders)
            .Where(x => x.IsMerchantSettled && !x.HasReturn && !x.HasProblem && ((x.DeliveredAt != null && x.DeliveredAt < cutoff) || (x.ReturnedToMerchantAt != null && x.ReturnedToMerchantAt < cutoff)))
            .ToListAsync(cancellationToken);

    public Task<List<OrderEvent>> GetOrderEventsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.OrderEvents).Where(x => orderIds.Contains(x.OrderId)).ToListAsync(cancellationToken);

    public Task<List<OrderProblem>> GetOrderProblemsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.OrderProblems).Where(x => orderIds.Contains(x.OrderId) && x.Status != ProblemStatus.Open).ToListAsync(cancellationToken);

    public Task AddOrderArchivesAsync(IEnumerable<OrderArchive> entities, CancellationToken cancellationToken = default)
        => _dbContext.OrdersArchive.AddRangeAsync(entities, cancellationToken);
    public Task AddOrderEventArchivesAsync(IEnumerable<OrderEventArchive> entities, CancellationToken cancellationToken = default)
        => _dbContext.OrderEventsArchive.AddRangeAsync(entities, cancellationToken);
    public Task AddOrderProblemArchivesAsync(IEnumerable<OrderProblemArchive> entities, CancellationToken cancellationToken = default)
        => _dbContext.OrderProblemsArchive.AddRangeAsync(entities, cancellationToken);

    public Task RemoveOrdersAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default) { _dbContext.Orders.RemoveRange(orders); return Task.CompletedTask; }
    public Task RemoveOrderEventsAsync(IEnumerable<OrderEvent> events, CancellationToken cancellationToken = default) { _dbContext.OrderEvents.RemoveRange(events); return Task.CompletedTask; }
    public Task RemoveOrderProblemsAsync(IEnumerable<OrderProblem> problems, CancellationToken cancellationToken = default) { _dbContext.OrderProblems.RemoveRange(problems); return Task.CompletedTask; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<T> Scoped<T>(IQueryable<T> q) where T : class
    {
        if (_requestContext.IsSaasAdmin) return q;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        q = q.Where(x => EF.Property<Guid>(x, "TenantId") == tenantId);
        if (_requestContext.IsCompanyAdmin) return q;
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return q.Where(x => EF.Property<Guid>(x, "BranchId") == branchId);
    }
}
