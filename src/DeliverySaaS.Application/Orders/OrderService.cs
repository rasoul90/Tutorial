using System.Text.Json;
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

    public async Task TransitionAsync(Guid orderId, OperationalState toState, bool deliveredWithReturn = false, CancellationToken cancellationToken = default)
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

        if (fromState == OperationalState.InDeliveryAgent && toState == OperationalState.Delivered)
        {
            await ApplyDeliveryFinancialsAsync(order, deliveredWithReturn, branchId, cancellationToken);
        }

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

    private async Task ApplyDeliveryFinancialsAsync(Order order, bool deliveredWithReturn, Guid branchId, CancellationToken cancellationToken)
    {
        if (!order.PricingCategoryId.HasValue || !order.GovernorateId.HasValue || !order.OrderSize.HasValue || !order.DeliveryAgentId.HasValue)
        {
            throw new InvalidOperationException("Order is missing pricing/delivery metadata for delivery settlement.");
        }

        var rate = await _orderRepository.GetPricingRateAsync(order.PricingCategoryId.Value, order.GovernorateId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Pricing rate not found for order delivery calculation.");

        var deliveryAgent = await _orderRepository.GetDeliveryAgentByIdAsync(order.DeliveryAgentId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Delivery agent not found.");

        var deliveryFee = order.OrderSize.Value switch
        {
            OrderSize.Size1 => rate.Size1Rate,
            OrderSize.Size2 => rate.Size2Rate,
            OrderSize.Size3 => rate.Size3Rate,
            OrderSize.Size4 => rate.Size4Rate,
            _ => throw new InvalidOperationException("Invalid order size.")
        };

        var deliveredPrice = order.AmountToCollect;
        var agentFee = deliveryAgent.DeliveryFeePerOrder;
        var merchantDue = deliveredPrice - deliveryFee;
        var companyProfit = deliveryFee - agentFee;

        order.DeliveredAt = DateTime.UtcNow;
        order.DeliveredPriceWithDelivery = deliveredPrice;
        order.DeliveryFeeApplied = deliveryFee;
        order.DeliveryAgentFeeApplied = agentFee;
        order.MerchantDueAmount = merchantDue;
        order.CompanyNetDeliveryProfit = companyProfit;

        if (deliveredWithReturn)
        {
            order.HasReturn = true;
            order.ReturnInitiatedAt = DateTime.UtcNow;
        }

        var payload = JsonSerializer.Serialize(new
        {
            order.DeliveredAt,
            order.DeliveredPriceWithDelivery,
            order.DeliveryFeeApplied,
            order.DeliveryAgentFeeApplied,
            order.MerchantDueAmount,
            order.CompanyNetDeliveryProfit,
            order.HasReturn,
            order.ReturnInitiatedAt
        });

        await _orderRepository.AddOrderEventAsync(new OrderEvent
        {
            OrderId = order.Id,
            EventType = deliveredWithReturn ? "DeliveredWithReturn" : "Delivered",
            Notes = payload,
            EventAt = DateTime.UtcNow,
            BranchId = branchId
        }, cancellationToken);
    }
}
