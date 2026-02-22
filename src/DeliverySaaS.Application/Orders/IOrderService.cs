using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Application.Orders;

public interface IOrderService
{
    Task TransitionAsync(Guid orderId, OperationalState toState, bool deliveredWithReturn = false, CancellationToken cancellationToken = default);
}
