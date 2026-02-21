using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<OrderProblem?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderProblems.FirstOrDefaultAsync(x => x.Id == problemId, cancellationToken);
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
        return _dbContext.OrderProblems.AnyAsync(x => x.OrderId == orderId && x.Status == ProblemStatus.Open, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
