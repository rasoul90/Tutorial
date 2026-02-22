using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Operations.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IMerchantPaymentsRepository
{
    Task<List<Order>> GetReadyOrdersForMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrdersByIdsAsync(Guid merchantId, IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken = default);
    Task AddPaymentAsync(MerchantPayment payment, CancellationToken cancellationToken = default);
    Task AddAllocationAsync(MerchantPaymentAllocation allocation, CancellationToken cancellationToken = default);
    Task AddOrderEventAsync(OrderEvent orderEvent, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
