using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Infrastructure.Persistence;
using DeliverySaaS.Infrastructure.Repositories;
using DeliverySaaS.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Tests.Integration;

public class ScopeEnforcementIntegrationTests
{
    [Fact]
    public async Task BranchScope_CannotSeeOtherBranches()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var branch1 = Guid.NewGuid();
        var branch2 = Guid.NewGuid();
        var merchant = Guid.NewGuid();

        await SeedOrder(dbName, tenantA, branch1, merchant, "A-1");
        await SeedOrder(dbName, tenantA, branch2, merchant, "A-2");

        var ctx = new TestRequestContext { TenantId = tenantA, BranchId = branch1, IsCompanyAdmin = false, IsSaasAdmin = false };
        await using var db = CreateDb(dbName, ctx);
        var repo = new OrderRepository(db, ctx);

        var dashboard = await repo.GetMerchantDashboardAsync(merchant);
        Assert.Equal(1, dashboard.TotalOrders);
    }

    [Fact]
    public async Task CompanyScope_CannotSeeOtherTenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var branch1 = Guid.NewGuid();
        var branch2 = Guid.NewGuid();
        var merchant = Guid.NewGuid();

        await SeedOrder(dbName, tenantA, branch1, merchant, "A-1");
        await SeedOrder(dbName, tenantA, branch2, merchant, "A-2");
        await SeedOrder(dbName, tenantB, Guid.NewGuid(), merchant, "B-1");

        var ctx = new TestRequestContext { TenantId = tenantA, BranchId = branch1, IsCompanyAdmin = true, IsSaasAdmin = false };
        await using var db = CreateDb(dbName, ctx);
        var repo = new OrderRepository(db, ctx);

        var dashboard = await repo.GetMerchantDashboardAsync(merchant);
        Assert.Equal(2, dashboard.TotalOrders);
    }

    private static async Task SeedOrder(string dbName, Guid tenantId, Guid branchId, Guid merchantId, string orderNumber)
    {
        var ctx = new TestRequestContext { TenantId = tenantId, BranchId = branchId, IsCompanyAdmin = false, IsSaasAdmin = false };
        await using var db = CreateDb(dbName, ctx);

        db.Orders.Add(new Order
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            MerchantId = merchantId,
            OrderNumber = orderNumber,
            CustomerName = "Customer",
            CustomerPhone = "010",
            Address = "Address",
            State = OperationalState.New,
            ProblemStatus = ProblemStatus.None,
            HasProblem = false
        });

        await db.SaveChangesAsync();
    }

    private static ApplicationDbContext CreateDb(string dbName, IRequestContext requestContext)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options, requestContext);
    }
}
