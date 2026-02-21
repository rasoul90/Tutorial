using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Tests.Common;

namespace DeliverySaaS.Tests.Unit;

public class ProblemResolutionTests
{
    [Fact]
    public async Task ResolveProblemAsync_PhoneChange_PatchesOrderAndClosesProblem()
    {
        var repo = new FakeOrderRepository { HasOpenProblemsResult = false };
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new OrderProblemService(repo, ctx, new FakeReferenceDataCacheService());

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BranchId = ctx.BranchId!.Value,
            MerchantId = Guid.NewGuid(),
            OrderNumber = "ORD-3",
            CustomerName = "N",
            CustomerPhone = "111",
            Address = "A",
            State = OperationalState.InDeliveryAgent,
            HasProblem = true,
            ProblemStatus = ProblemStatus.Open
        };

        var problem = new OrderProblem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProblemCatalogId = Guid.NewGuid(),
            BranchId = ctx.BranchId.Value,
            Status = ProblemStatus.Open
        };

        repo.Orders[order.Id] = order;
        repo.Problems[problem.Id] = problem;

        await service.ResolveProblemAsync(problem.Id, DynamicResolutionType.PHONE_CHANGE, "222", null, null, null);

        Assert.Equal("222", order.CustomerPhone);
        Assert.Equal(ProblemStatus.Resolved, problem.Status);
        Assert.False(order.HasProblem);
        Assert.Equal(ProblemStatus.Resolved, order.ProblemStatus);
        Assert.Contains(repo.Events, e => e.EventType == "FieldPatch");
    }
}
