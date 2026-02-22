using DeliverySaaS.Application.Payments;
using DeliverySaaS.Infrastructure.Persistence;
using DeliverySaaS.Infrastructure.QueryServices;
using DeliverySaaS.Infrastructure.Repositories;
using DeliverySaaS.Tests.Common;
using DeliverySaaS.Domain.Accounting.Enums;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Tests.Unit;

public class PaymentsAndLedgerTests
{
    [Fact]
    public async Task FifoAllocation_DistributesToOldestReadyOrders()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var ctx = new TestRequestContext { TenantId = tenantId, BranchId = branchId };
        await using var db = CreateDb(nameof(FifoAllocation_DistributesToOldestReadyOrders), ctx);

        var older = NewOrder(tenantId, branchId, merchantId, DateTime.UtcNow.AddDays(-2), 100m);
        var newer = NewOrder(tenantId, branchId, merchantId, DateTime.UtcNow.AddDays(-1), 150m);
        db.Orders.AddRange(older, newer);
        await db.SaveChangesAsync();

        var service = new MerchantPaymentsService(new MerchantPaymentsRepository(db, ctx), ctx);
        await service.CreatePaymentAsync(new CreateMerchantPaymentRequest
        {
            MerchantId = merchantId,
            PaymentDate = DateTime.UtcNow,
            Amount = 120m,
            Method = PaymentMethod.InPerson,
            CreatedByUserId = Guid.NewGuid(),
            AutoAllocateFifo = true
        });

        Assert.Equal(0m, older.MerchantRemainingAmount);
        Assert.Equal(130m, newer.MerchantRemainingAmount);
    }

    [Fact]
    public async Task PartialPayment_UpdatesRemainingAmount()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var ctx = new TestRequestContext { TenantId = tenantId, BranchId = branchId };
        await using var db = CreateDb(nameof(PartialPayment_UpdatesRemainingAmount), ctx);

        var order = NewOrder(tenantId, branchId, merchantId, DateTime.UtcNow.AddDays(-1), 200m);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var service = new MerchantPaymentsService(new MerchantPaymentsRepository(db, ctx), ctx);
        await service.CreatePaymentAsync(new CreateMerchantPaymentRequest
        {
            MerchantId = merchantId,
            PaymentDate = DateTime.UtcNow,
            Amount = 50m,
            Method = PaymentMethod.Courier,
            CreatedByUserId = Guid.NewGuid(),
            AutoAllocateFifo = true
        });

        Assert.Equal(50m, order.MerchantPaidAmount);
        Assert.Equal(150m, order.MerchantRemainingAmount);
        Assert.Equal(MerchantSettlementStatus.PartiallyPaid, order.MerchantSettlementStatus);
    }

    [Fact]
    public async Task Ledger_ComputesRunningBalance()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var ctx = new TestRequestContext { TenantId = tenantId, BranchId = branchId };
        await using var db = CreateDb(nameof(Ledger_ComputesRunningBalance), ctx);

        db.Orders.Add(NewOrder(tenantId, branchId, merchantId, DateTime.UtcNow.AddDays(-3), 100m));
        db.MerchantPayments.Add(new Domain.Accounting.Entities.MerchantPayment
        {
            MerchantId = merchantId,
            BranchId = branchId,
            PaymentDate = DateTime.UtcNow.AddDays(-2),
            Amount = 40m,
            Method = PaymentMethod.ETransfer,
            CreatedByUserId = Guid.NewGuid()
        });
        db.Orders.Add(NewOrder(tenantId, branchId, merchantId, DateTime.UtcNow.AddDays(-1), 80m));
        await db.SaveChangesAsync();

        var ledger = await new MerchantLedgerQueryService(db, ctx).GetLedgerAsync(merchantId, DateTime.UtcNow.AddDays(-4), DateTime.UtcNow, 1, 20);

        Assert.Equal(0m, ledger.OpeningBalance);
        Assert.Equal(180m, ledger.Summary.TotalDebit);
        Assert.Equal(40m, ledger.Summary.TotalCredit);
        Assert.Equal(140m, ledger.Summary.ClosingBalance);
        Assert.Equal(140m, ledger.Rows.Last().Balance);
    }

    private static ApplicationDbContext CreateDb(string dbName, TestRequestContext requestContext)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options, requestContext);
    }

    private static Order NewOrder(Guid tenantId, Guid branchId, Guid merchantId, DateTime deliveredAt, decimal due) => new()
    {
        TenantId = tenantId,
        BranchId = branchId,
        MerchantId = merchantId,
        OrderNumber = Guid.NewGuid().ToString("N"),
        CustomerName = "C",
        CustomerPhone = "0",
        Address = "A",
        AmountToCollect = due,
        DeliveredAt = deliveredAt,
        MerchantDueAmount = due,
        MerchantRemainingAmount = due,
        IsDeliveryAgentSettled = true,
        MerchantSettlementStatus = MerchantSettlementStatus.Ready
    };
}
