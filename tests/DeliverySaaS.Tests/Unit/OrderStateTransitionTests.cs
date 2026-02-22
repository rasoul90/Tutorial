using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Tests.Common;

namespace DeliverySaaS.Tests.Unit;

public class OrderStateTransitionTests
{
    [Fact]
    public async Task TransitionAsync_AllowsConfiguredTransition_AndCreatesEvent()
    {
        var repo = new FakeOrderRepository();
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new OrderService(repo, ctx);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BranchId = ctx.BranchId!.Value,
            MerchantId = Guid.NewGuid(),
            OrderNumber = "ORD-1",
            CustomerName = "Test",
            CustomerPhone = "0100",
            Address = "Addr",
            State = OperationalState.New
        };
        repo.Orders[order.Id] = order;

        await service.TransitionAsync(order.Id, OperationalState.InPickupAgent);

        Assert.Equal(OperationalState.InPickupAgent, order.State);
        Assert.Single(repo.Events);
        Assert.Equal("StateTransition", repo.Events[0].EventType);
    }

    [Fact]
    public async Task TransitionAsync_RejectsInvalidTransition()
    {
        var repo = new FakeOrderRepository();
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new OrderService(repo, ctx);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BranchId = ctx.BranchId!.Value,
            MerchantId = Guid.NewGuid(),
            OrderNumber = "ORD-2",
            CustomerName = "Test",
            CustomerPhone = "0100",
            Address = "Addr",
            State = OperationalState.New
        };
        repo.Orders[order.Id] = order;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransitionAsync(order.Id, OperationalState.Delivered));
    }

    [Fact]
    public async Task TransitionAsync_ToDelivered_ComputesFinancialsAndCreatesDeliveredWithReturnEvent()
    {
        var repo = new FakeOrderRepository();
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new OrderService(repo, ctx);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BranchId = ctx.BranchId!.Value,
            MerchantId = Guid.NewGuid(),
            DeliveryAgentId = Guid.NewGuid(),
            GovernorateId = Guid.NewGuid(),
            PricingCategoryId = Guid.NewGuid(),
            OrderSize = OrderSize.Size1,
            AmountToCollect = 100m,
            OrderNumber = "ORD-3",
            CustomerName = "Test",
            CustomerPhone = "0100",
            Address = "Addr",
            State = OperationalState.InDeliveryAgent
        };
        repo.Orders[order.Id] = order;

        await service.TransitionAsync(order.Id, OperationalState.Delivered, deliveredWithReturn: true);

        Assert.Equal(OperationalState.Delivered, order.State);
        Assert.Equal(100m, order.DeliveredPriceWithDelivery);
        Assert.Equal(0m, order.DeliveryFeeApplied);
        Assert.Equal(0m, order.DeliveryAgentFeeApplied);
        Assert.Equal(100m, order.MerchantDueAmount);
        Assert.Equal(0m, order.CompanyNetDeliveryProfit);
        Assert.True(order.HasReturn);
        Assert.NotNull(order.DeliveredAt);
        Assert.NotNull(order.ReturnInitiatedAt);
        Assert.Contains(repo.Events, e => e.OrderId == order.Id && e.EventType == "DeliveredWithReturn");
        Assert.Contains(repo.Events, e => e.OrderId == order.Id && e.EventType == "StateTransition");
    }
}
