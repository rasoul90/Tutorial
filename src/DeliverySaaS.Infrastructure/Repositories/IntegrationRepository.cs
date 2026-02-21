using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Integration.Entities;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class IntegrationRepository : IIntegrationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public IntegrationRepository(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public Task AddPartnerConnectionAsync(PartnerConnection entity, CancellationToken cancellationToken = default)
        => _dbContext.PartnerConnections.AddAsync(entity, cancellationToken).AsTask();

    public Task<PartnerConnection?> GetPartnerConnectionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.PartnerConnections.AsNoTracking()).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PartnerConnection?> GetPartnerConnectionByNameAsync(string partnerName, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.PartnerConnections.AsNoTracking()).FirstOrDefaultAsync(x => x.PartnerName == partnerName, cancellationToken);

    public Task<RoutingRule?> GetRoutingRuleByGovernorateAsync(Guid governorateId, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.RoutingRules.AsNoTracking()).Where(x => x.IsActive && x.GovernorateId == governorateId).OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

    public Task AddOrderHandoffAsync(OrderHandoff handoff, CancellationToken cancellationToken = default)
        => _dbContext.OrderHandoffs.AddAsync(handoff, cancellationToken).AsTask();

    public Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        => _dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();

    public Task AddInboxMessageAsync(InboxMessage message, CancellationToken cancellationToken = default)
        => _dbContext.InboxMessages.AddAsync(message, cancellationToken).AsTask();

    public Task<List<OutboxMessage>> GetPendingOutboxAsync(int take, CancellationToken cancellationToken = default)
    {
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        return _dbContext.OutboxMessages
            .Where(x => x.TenantId == tenantId && x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> InboxMessageExistsAsync(string messageType, string payload, CancellationToken cancellationToken = default)
    {
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        return _dbContext.InboxMessages.AsNoTracking().AnyAsync(x => x.TenantId == tenantId && x.Type == messageType && x.Payload == payload, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<T> Scoped<T>(IQueryable<T> query) where T : class
    {
        if (_requestContext.IsSaasAdmin) return query;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");

        if (_requestContext.IsCompanyAdmin)
            return query.Where(x => EF.Property<Guid>(x, "TenantId") == tenantId);

        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return query.Where(x => EF.Property<Guid>(x, "TenantId") == tenantId && EF.Property<Guid>(x, "BranchId") == branchId);
    }
}
