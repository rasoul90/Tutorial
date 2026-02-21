using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Application.Orders;

public class OrderService : IOrderService
{
    private static readonly HashSet<(OperationalState From, OperationalState To)> AllowedTransitions =
    [
        (OperationalState.New, OperationalState.InPickupAgent),
        (OperationalState.InPickupAgent, OperationalState.InSortingHub),
        (OperationalState.InSortingHub, OperationalState.InDeliveryAgent),
        (OperationalState.InDeliveryAgent, OperationalState.Delivered),
        (OperationalState.InDeliveryAgent, OperationalState.ReturnedOrders),
        (OperationalState.ReturnedOrders, OperationalState.ReturnSortingHub),
        (OperationalState.ReturnSortingHub, OperationalState.ReturnedToMerchant)
    ];

    private readonly IOrderRepository _orderRepository;
    private readonly IRequestContext _requestContext;

    public OrderService(IOrderRepository orderRepository, IRequestContext requestContext)
    {
        _orderRepository = orderRepository;
        _requestContext = requestContext;
    }

    public async Task TransitionAsync(Guid orderId, OperationalState toState, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found.");

        if (!AllowedTransitions.Contains((order.State, toState)))
        {
            throw new InvalidOperationException($"Transition from {order.State} to {toState} is not allowed.");
        }

        var fromState = order.State;
        var branchId = _requestContext.BranchId
            ?? throw new InvalidOperationException("BranchId is required for order transition.");

        order.State = toState;

        var orderEvent = new OrderEvent
        {
            OrderId = order.Id,
            EventType = "StateTransition",
            Notes = $"{fromState} -> {toState}",
            EventAt = DateTime.UtcNow,
            BranchId = branchId
        };

        await _orderRepository.AddOrderEventAsync(orderEvent, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }
}
