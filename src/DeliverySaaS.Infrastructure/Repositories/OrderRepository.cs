using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
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
    {
        return _dbContext.Orders.AddAsync(order, cancellationToken).AsTask();
    }

    public Task<int> CountOrdersByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.Orders.AsQueryable());
        return query.CountAsync(x => x.MerchantId == merchantId, cancellationToken);
    }

    public Task<int> CountOpenProblemsByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var orders = ApplyScope(_dbContext.Orders.AsQueryable()).Where(x => x.MerchantId == merchantId).Select(x => x.Id);
        var problems = ApplyScope(_dbContext.OrderProblems.AsQueryable());
        return problems.CountAsync(x => orders.Contains(x.OrderId) && x.Status == ProblemStatus.Open, cancellationToken);
    }

    public Task<List<Order>> GetPickupTaskListAsync(int take, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.Orders.AsQueryable())
            .Where(x => x.State == OperationalState.New || x.State == OperationalState.InPickupAgent)
            .OrderBy(x => x.CreatedAt)
            .Take(take);
        return query.ToListAsync(cancellationToken);
    }

    public Task<OrderProblem?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.OrderProblems.AsQueryable());
        return query.FirstOrDefaultAsync(x => x.Id == problemId, cancellationToken);
    }

    public Task AddOrderProblemAsync(OrderProblem orderProblem, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderProblems.AddAsync(orderProblem, cancellationToken).AsTask();
    }

    public Task AddOrderEventAsync(OrderEvent orderEvent, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderEvents.AddAsync(orderEvent, cancellationToken).AsTask();
    }

    public Task<bool> HasOpenProblemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(_dbContext.OrderProblems.AsQueryable());
        return query.AnyAsync(x => x.OrderId == orderId && x.Status == ProblemStatus.Open, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

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
