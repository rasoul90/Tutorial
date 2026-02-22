using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Pricing.Entities;

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

    Task<Merchant?> GetMerchantByIdAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<DeliveryAgent?> GetDeliveryAgentByIdAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default);
    Task<PricingRate?> GetPricingRateAsync(Guid pricingCategoryId, Guid governorateId, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrdersForDeliveryAgentSettlementAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrdersAvailableForMerchantSettlementAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrdersPendingDeliveryAgentSettlementAsync(CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrdersAvailableForMerchantSettlementListAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetProfitabilitySumAsync(Guid? branchId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
