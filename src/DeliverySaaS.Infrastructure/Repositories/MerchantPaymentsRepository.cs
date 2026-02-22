using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class MerchantPaymentsRepository : IMerchantPaymentsRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public MerchantPaymentsRepository(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public Task<List<Order>> GetReadyOrdersForMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.Orders)
            .Where(x => x.MerchantId == merchantId && x.DeliveredAt != null && x.IsDeliveryAgentSettled && x.MerchantRemainingAmount > 0)
            .OrderBy(x => x.DeliveredAt)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<Order>> GetOrdersByIdsAsync(Guid merchantId, IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken = default)
        => Scoped(_dbContext.Orders)
            .Where(x => x.MerchantId == merchantId && orderIds.Contains(x.Id) && x.DeliveredAt != null)
            .ToListAsync(cancellationToken);

    public Task AddPaymentAsync(MerchantPayment payment, CancellationToken cancellationToken = default)
        => _dbContext.MerchantPayments.AddAsync(payment, cancellationToken).AsTask();

    public Task AddAllocationAsync(MerchantPaymentAllocation allocation, CancellationToken cancellationToken = default)
        => _dbContext.MerchantPaymentAllocations.AddAsync(allocation, cancellationToken).AsTask();

    public Task AddOrderEventAsync(OrderEvent orderEvent, CancellationToken cancellationToken = default)
        => _dbContext.OrderEvents.AddAsync(orderEvent, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<T> Scoped<T>(IQueryable<T> query) where T : class
    {
        if (_requestContext.IsSaasAdmin) return query;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        query = query.Where(x => EF.Property<Guid>(x, "TenantId") == tenantId);
        if (_requestContext.IsCompanyAdmin) return query;
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return query.Where(x => EF.Property<Guid>(x, "BranchId") == branchId);
    }
}
