using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Tests.Common;

public class TestRequestContext : IRequestContext
{
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsCompanyAdmin { get; set; }
    public bool IsSaasAdmin { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}

public class FakeOrderRepository : IOrderRepository
{
    public Dictionary<Guid, Order> Orders { get; } = new();
    public Dictionary<Guid, OrderProblem> Problems { get; } = new();
    public List<OrderEvent> Events { get; } = new();
    public bool HasOpenProblemsResult { get; set; }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Orders.GetValueOrDefault(id));

    public Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        Orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<int> CountOrdersByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
        => Task.FromResult(Orders.Values.Count(x => x.MerchantId == merchantId));

    public Task<int> CountOpenProblemsByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var orderIds = Orders.Values.Where(x => x.MerchantId == merchantId).Select(x => x.Id).ToHashSet();
        return Task.FromResult(Problems.Values.Count(x => orderIds.Contains(x.OrderId) && x.Status == ProblemStatus.Open));
    }

    public Task<List<Order>> GetPickupTaskListAsync(int take, CancellationToken cancellationToken = default)
        => Task.FromResult(Orders.Values.Take(take).ToList());

    public Task<OrderProblem?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken = default)
        => Task.FromResult(Problems.GetValueOrDefault(problemId));

    public Task AddOrderProblemAsync(OrderProblem orderProblem, CancellationToken cancellationToken = default)
    {
        Problems[orderProblem.Id] = orderProblem;
        return Task.CompletedTask;
    }

    public Task AddOrderEventAsync(OrderEvent orderEvent, CancellationToken cancellationToken = default)
    {
        Events.Add(orderEvent);
        return Task.CompletedTask;
    }

    public Task<bool> HasOpenProblemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        => Task.FromResult(HasOpenProblemsResult);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class FakeAccountingRepository : IAccountingRepository
{
    public Dictionary<Guid, MerchantSettlementRequest> Settlements { get; } = new();
    public List<MerchantInvoice> Invoices { get; } = new();
    public List<AuditEntry> Audits { get; } = new();

    public Task<MerchantSettlementRequest?> GetSettlementRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Settlements.GetValueOrDefault(id));

    public Task AddSettlementRequestAsync(MerchantSettlementRequest settlementRequest, CancellationToken cancellationToken = default)
    {
        Settlements[settlementRequest.Id] = settlementRequest;
        return Task.CompletedTask;
    }

    public Task AddInvoiceAsync(MerchantInvoice invoice, CancellationToken cancellationToken = default)
    {
        Invoices.Add(invoice);
        return Task.CompletedTask;
    }

    public Task AddDeliveryReconciliationAsync(DeliveryReconciliation reconciliation, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task AddPayrollAsync(Payroll payroll, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task AddExpenseAsync(Expense expense, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task AddAuditEntryAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default)
    {
        Audits.Add(auditEntry);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
