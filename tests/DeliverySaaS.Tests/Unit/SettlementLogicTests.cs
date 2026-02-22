using DeliverySaaS.Application.Accounting;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Tests.Common;

namespace DeliverySaaS.Tests.Unit;

public class SettlementLogicTests
{
    [Fact]
    public async Task ApproveSettlementAndGenerateInvoice_ApprovesSettlementAndCreatesInvoiceAndAudit()
    {
        var repo = new FakeAccountingRepository();
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new AccountingService(repo, new FakeOrderRepository(), ctx);

        var settlement = new MerchantSettlementRequest
        {
            Id = Guid.NewGuid(),
            MerchantId = Guid.NewGuid(),
            Amount = 150m,
            Status = "Pending",
            BranchId = ctx.BranchId!.Value
        };
        repo.Settlements[settlement.Id] = settlement;

        var invoiceId = await service.ApproveSettlementAndGenerateInvoiceAsync(settlement.Id);

        Assert.Equal("Approved", settlement.Status);
        Assert.Contains(repo.Invoices, x => x.Id == invoiceId && x.TotalAmount == settlement.Amount);
        Assert.True(repo.Audits.Count >= 2);
    }

    [Fact]
    public async Task RecordDeliveryReconciliation_SettlesOrders_AndMovesReturnedOnesToHub()
    {
        var accountingRepo = new FakeAccountingRepository();
        var orderRepo = new FakeOrderRepository();
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new AccountingService(accountingRepo, orderRepo, ctx);
        var deliveryAgentId = Guid.NewGuid();

        var deliveredOrder = new Order
        {
            Id = Guid.NewGuid(),
            DeliveryAgentId = deliveryAgentId,
            State = OperationalState.Delivered,
            BranchId = ctx.BranchId!.Value,
            MerchantId = Guid.NewGuid(),
            OrderNumber = "O-1",
            CustomerName = "A",
            CustomerPhone = "1",
            Address = "X",
            DeliveredAt = DateTime.UtcNow
        };

        var returnedOrder = new Order
        {
            Id = Guid.NewGuid(),
            DeliveryAgentId = deliveryAgentId,
            State = OperationalState.Delivered,
            HasReturn = true,
            BranchId = ctx.BranchId!.Value,
            MerchantId = Guid.NewGuid(),
            OrderNumber = "O-2",
            CustomerName = "B",
            CustomerPhone = "2",
            Address = "Y",
            DeliveredAt = DateTime.UtcNow
        };

        orderRepo.DeliveryAgentSettlementOrders.Add(deliveredOrder);
        orderRepo.DeliveryAgentSettlementOrders.Add(returnedOrder);

        await service.RecordDeliveryReconciliationAsync(deliveryAgentId, 500m, 450m);

        Assert.True(deliveredOrder.IsDeliveryAgentSettled);
        Assert.True(returnedOrder.IsDeliveryAgentSettled);
        Assert.Equal(OperationalState.ReturnSortingHub, returnedOrder.State);
        Assert.Contains(orderRepo.Events, e => e.OrderId == deliveredOrder.Id && e.EventType == "DeliveryAgentSettled");
        Assert.Contains(orderRepo.Events, e => e.OrderId == returnedOrder.Id && e.EventType == "ReturnEnteredHubAfterDeliverySettlement");
    }

    [Fact]
    public async Task CreateMerchantSettlementRequest_ComputesAmountAndMarksMerchantSettled()
    {
        var accountingRepo = new FakeAccountingRepository();
        var orderRepo = new FakeOrderRepository();
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new AccountingService(accountingRepo, orderRepo, ctx);
        var merchantId = Guid.NewGuid();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            State = OperationalState.Delivered,
            BranchId = ctx.BranchId!.Value,
            OrderNumber = "M-1",
            CustomerName = "M",
            CustomerPhone = "3",
            Address = "Z",
            DeliveredAt = DateTime.UtcNow,
            IsDeliveryAgentSettled = true,
            MerchantDueAmount = 220m
        };

        orderRepo.MerchantSettlementOrders.Add(order);

        var requestId = await service.CreateMerchantSettlementRequestAsync(merchantId, 1m);

        Assert.True(order.IsMerchantSettled);
        Assert.NotNull(order.MerchantSettledAt);
        Assert.True(accountingRepo.Settlements.TryGetValue(requestId, out var settlement));
        Assert.Equal(220m, settlement!.Amount);
        Assert.Contains(orderRepo.Events, e => e.OrderId == order.Id && e.EventType == "MerchantSettled");
    }
}
