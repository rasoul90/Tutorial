using DeliverySaaS.Domain.Operations.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task<int> CountOrdersByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<int> CountOpenProblemsByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetPickupTaskListAsync(int take, CancellationToken cancellationToken = default);
    Task<OrderProblem?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken = default);
    Task AddOrderProblemAsync(OrderProblem orderProblem, CancellationToken cancellationToken = default);
    Task AddOrderEventAsync(OrderEvent orderEvent, CancellationToken cancellationToken = default);
    Task<bool> HasOpenProblemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
