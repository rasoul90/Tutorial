using DeliverySaaS.Infrastructure.Persistence;
using DeliverySaaS.Infrastructure.QueryServices;
using DeliverySaaS.Tests.Common;
using DeliverySaaS.Domain.Operations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeliverySaaS.Tests.Unit;

public class ReportsQueryServicesTests
{
    [Fact]
    public async Task MerchantStatement_SummarySums_AreCalculated()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var ctx = new TestRequestContext { TenantId = tenantId, BranchId = branchId };
        await using var db = CreateDb(nameof(MerchantStatement_SummarySums_AreCalculated), ctx);
        db.Orders.AddRange(
            NewOrder(tenantId, branchId, merchantId, null, 100m, 10m, 90m, true, true),
            NewOrder(tenantId, branchId, merchantId, null, 200m, 20m, 180m, true, false),
            NewOrder(tenantId, branchId, merchantId, null, 300m, 30m, 270m, false, false));
        await db.SaveChangesAsync();

        var service = new MerchantStatementsQueryService(db, ctx, NullLogger<MerchantStatementsQueryService>.Instance);
        var result = await service.GetStatementAsync(merchantId, DateTime.UtcNow.Date.AddDays(-2), DateTime.UtcNow.Date.AddDays(1), 1, 20, false);

        Assert.Equal(600m, result.Summary.TotalDeliveredValue);
        Assert.Equal(60m, result.Summary.TotalDeliveryFee);
        Assert.Equal(540m, result.Summary.TotalMerchantDue);
        Assert.Equal(90m, result.Summary.TotalSettledToMerchant);
        Assert.Equal(180m, result.Summary.TotalPendingToMerchant);
        Assert.Equal(270m, result.Summary.TotalNotReady);
    }

    [Fact]
    public async Task DeliveryAgentStatement_SummarySums_AreCalculated()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var ctx = new TestRequestContext { TenantId = tenantId, BranchId = branchId };
        await using var db = CreateDb(nameof(DeliveryAgentStatement_SummarySums_AreCalculated), ctx);
        db.Orders.AddRange(
            NewOrder(tenantId, branchId, Guid.NewGuid(), agentId, 100m, 10m, 90m, true, false, 4m, 6m),
            NewOrder(tenantId, branchId, Guid.NewGuid(), agentId, 200m, 20m, 180m, false, false, 8m, 12m));
        await db.SaveChangesAsync();

        var service = new DeliveryAgentStatementsQueryService(db, ctx, NullLogger<DeliveryAgentStatementsQueryService>.Instance);
        var result = await service.GetStatementAsync(agentId, DateTime.UtcNow.Date.AddDays(-2), DateTime.UtcNow.Date.AddDays(1), 1, 20, false);

        Assert.Equal(300m, result.Summary.TotalDeliveredValue);
        Assert.Equal(30m, result.Summary.TotalDeliveryFee);
        Assert.Equal(12m, result.Summary.TotalAgentFees);
        Assert.Equal(18m, result.Summary.TotalCompanyNetProfit);
        Assert.Equal(1, result.Summary.TotalPendingSettlementCount);
        Assert.Equal(200m, result.Summary.TotalPendingSettlementAmount);
    }

    [Fact]
    public async Task BranchFinancialSummary_Sums_AreCalculated()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var ctx = new TestRequestContext { TenantId = tenantId, BranchId = branchId };
        await using var db = CreateDb(nameof(BranchFinancialSummary_Sums_AreCalculated), ctx);
        db.Orders.AddRange(
            NewOrder(tenantId, branchId, Guid.NewGuid(), Guid.NewGuid(), 100m, 10m, 90m, false, false, 4m, 6m),
            NewOrder(tenantId, branchId, Guid.NewGuid(), Guid.NewGuid(), 200m, 20m, 180m, true, false, 8m, 12m));
        await db.SaveChangesAsync();

        var service = new FinancialReportsQueryService(db, ctx);
        var summary = await service.GetBranchFinancialSummaryAsync(DateTime.UtcNow.Date.AddDays(-2), DateTime.UtcNow.Date.AddDays(1));

        Assert.Equal(2, summary.TotalOrdersDelivered);
        Assert.Equal(300m, summary.TotalDeliveredValue);
        Assert.Equal(30m, summary.TotalDeliveryFee);
        Assert.Equal(12m, summary.TotalDeliveryAgentFees);
        Assert.Equal(18m, summary.TotalCompanyNetProfit);
        Assert.Equal(270m, summary.TotalMerchantDue);
        Assert.Equal(1, summary.TotalUnsettledWithDeliveryAgentsCount);
        Assert.Equal(100m, summary.TotalUnsettledWithDeliveryAgentsAmount);
        Assert.Equal(1, summary.TotalUnsettledWithMerchantsCount);
        Assert.Equal(180m, summary.TotalUnsettledWithMerchantsAmount);
    }

    private static ApplicationDbContext CreateDb(string dbName, TestRequestContext requestContext)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options, requestContext);
    }

    private static Order NewOrder(Guid tenantId, Guid branchId, Guid merchantId, Guid? agentId, decimal delivered, decimal deliveryFee, decimal merchantDue, bool agentSettled, bool merchantSettled, decimal agentFee = 0m, decimal companyProfit = 0m)
        => new()
        {
            TenantId = tenantId,
            BranchId = branchId,
            MerchantId = merchantId,
            DeliveryAgentId = agentId,
            OrderNumber = Guid.NewGuid().ToString("N"),
            CustomerName = "Customer",
            CustomerPhone = "0100",
            Address = "Addr",
            AmountToCollect = delivered,
            DeliveredAt = DateTime.UtcNow,
            DeliveredPriceWithDelivery = delivered,
            DeliveryFeeApplied = deliveryFee,
            MerchantDueAmount = merchantDue,
            DeliveryAgentFeeApplied = agentFee,
            CompanyNetDeliveryProfit = companyProfit,
            IsDeliveryAgentSettled = agentSettled,
            IsMerchantSettled = merchantSettled
        };
}
