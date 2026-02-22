using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Domain.Pricing.Entities;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public OrderRepository(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.Orders.AsQueryable());
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
        => _dbContext.Orders.AddAsync(order, cancellationToken).AsTask();

    public async Task<MerchantDashboardDto> GetMerchantDashboardAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.Orders.AsNoTracking())
            .Where(x => x.MerchantId == merchantId)
            .GroupBy(_ => 1)
            .Select(g => new MerchantDashboardDto(g.Count(), g.Count(x => x.ProblemStatus == ProblemStatus.Open)));

        return await query.FirstOrDefaultAsync(cancellationToken) ?? new MerchantDashboardDto(0, 0);
    }

    public async Task<IReadOnlyList<PickupTaskDto>> GetPickupTaskListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPage = pageNumber <= 0 ? 1 : pageNumber;
        var normalizedSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);

        var query = ApplyScope(_dbContext.Orders.AsNoTracking())
            .Where(x => x.State == OperationalState.New || x.State == OperationalState.InPickupAgent)
            .OrderBy(x => x.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .Select(x => new PickupTaskDto(x.Id, x.OrderNumber, x.CustomerName, x.CustomerPhone, x.Address, x.State));

        return await query.ToListAsync(cancellationToken);
    }

    public Task<OrderProblem?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.OrderProblems.AsQueryable());
        return query.FirstOrDefaultAsync(x => x.Id == problemId, cancellationToken);
    }

    public Task AddOrderProblemAsync(OrderProblem orderProblem, CancellationToken cancellationToken = default)
        => _dbContext.OrderProblems.AddAsync(orderProblem, cancellationToken).AsTask();

    public Task AddOrderEventAsync(OrderEvent orderEvent, CancellationToken cancellationToken = default)
        => _dbContext.OrderEvents.AddAsync(orderEvent, cancellationToken).AsTask();

    public Task<bool> HasOpenProblemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.OrderProblems.AsNoTracking());
        return query.AnyAsync(x => x.OrderId == orderId && x.Status == ProblemStatus.Open, cancellationToken);
    }

    public Task<Merchant?> GetMerchantByIdAsync(Guid merchantId, CancellationToken cancellationToken = default)
        => ApplyScope(_dbContext.Merchants.AsNoTracking()).FirstOrDefaultAsync(x => x.Id == merchantId, cancellationToken);

    public Task<DeliveryAgent?> GetDeliveryAgentByIdAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default)
        => ApplyScope(_dbContext.DeliveryAgents.AsNoTracking()).FirstOrDefaultAsync(x => x.Id == deliveryAgentId, cancellationToken);

    public Task<PricingRate?> GetPricingRateAsync(Guid pricingCategoryId, Guid governorateId, CancellationToken cancellationToken = default)
        => ApplyScope(_dbContext.PricingRates.AsNoTracking())
            .Where(x => x.PricingCategoryId == pricingCategoryId && x.GovernorateId == governorateId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<Order>> GetOrdersForDeliveryAgentSettlementAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default)
        => ApplyScope(_dbContext.Orders.AsQueryable())
            .Where(x => x.DeliveryAgentId == deliveryAgentId && x.DeliveredAt != null && !x.IsDeliveryAgentSettled)
            .ToListAsync(cancellationToken);

    public Task<List<Order>> GetOrdersAvailableForMerchantSettlementAsync(Guid merchantId, CancellationToken cancellationToken = default)
        => ApplyScope(_dbContext.Orders.AsQueryable())
            .Where(x => x.MerchantId == merchantId && x.IsDeliveryAgentSettled && !x.IsMerchantSettled && x.DeliveredAt != null)
            .ToListAsync(cancellationToken);

    public Task<List<Order>> GetOrdersPendingDeliveryAgentSettlementAsync(CancellationToken cancellationToken = default)
        => ApplyScope(_dbContext.Orders.AsNoTracking())
            .Where(x => x.DeliveredAt != null && !x.IsDeliveryAgentSettled)
            .OrderByDescending(x => x.DeliveredAt)
            .ToListAsync(cancellationToken);

    public Task<List<Order>> GetOrdersAvailableForMerchantSettlementListAsync(CancellationToken cancellationToken = default)
        => ApplyScope(_dbContext.Orders.AsNoTracking())
            .Where(x => x.DeliveredAt != null && x.IsDeliveryAgentSettled && !x.IsMerchantSettled)
            .OrderByDescending(x => x.DeliveredAt)
            .ToListAsync(cancellationToken);

    public Task<decimal> GetProfitabilitySumAsync(Guid? branchId, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.Orders.AsNoTracking()).Where(x => x.CompanyNetDeliveryProfit.HasValue);
        if (_requestContext.IsCompanyAdmin && branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        return query.SumAsync(x => x.CompanyNetDeliveryProfit ?? 0m, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<T> ApplyScope<T>(IQueryable<T> query) where T : class
    {
        var tenantId = _requestContext.TenantId;

        if (_requestContext.IsSaasAdmin)
        {
            return query;
        }

        if (!tenantId.HasValue)
        {
            throw new InvalidOperationException("TenantId is required.");
        }

        if (_requestContext.IsCompanyAdmin)
        {
            return query.Where(x => EF.Property<Guid>(x, "TenantId") == tenantId.Value);
        }

        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required for branch scope.");
        return query.Where(x => EF.Property<Guid>(x, "TenantId") == tenantId.Value && EF.Property<Guid>(x, "BranchId") == branchId);
    }
}
