using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task<MerchantDashboardDto> GetMerchantDashboardAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PickupTaskDto>> GetPickupTaskListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderProblem?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken = default);
    Task AddOrderProblemAsync(OrderProblem orderProblem, CancellationToken cancellationToken = default);
    Task AddOrderEventAsync(OrderEvent orderEvent, CancellationToken cancellationToken = default);
    Task<bool> HasOpenProblemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
