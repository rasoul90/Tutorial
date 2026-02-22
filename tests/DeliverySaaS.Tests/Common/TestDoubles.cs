using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Application.Common.Models;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using DeliverySaaS.Domain.Pricing.Entities;

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
    public List<Order> DeliveryAgentSettlementOrders { get; } = new();
    public List<Order> MerchantSettlementOrders { get; } = new();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Orders.GetValueOrDefault(id));

    public Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        Orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<MerchantDashboardDto> GetMerchantDashboardAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var merchantOrders = Orders.Values.Where(x => x.MerchantId == merchantId).ToList();
        var open = merchantOrders.Count(x => x.ProblemStatus == ProblemStatus.Open);
        return Task.FromResult(new MerchantDashboardDto(merchantOrders.Count, open));
    }


    public Task<IReadOnlyList<PickupTaskDto>> GetPickupTaskListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var page = pageNumber <= 0 ? 1 : pageNumber;
        var size = pageSize <= 0 ? 50 : pageSize;
        var items = Orders.Values
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new PickupTaskDto(x.Id, x.OrderNumber, x.CustomerName, x.CustomerPhone, x.Address, x.State))
            .ToList();

        return Task.FromResult((IReadOnlyList<PickupTaskDto>)items);
    }

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


    public Task<Merchant?> GetMerchantByIdAsync(Guid merchantId, CancellationToken cancellationToken = default)
        => Task.FromResult<Merchant?>(null);

    public Task<DeliveryAgent?> GetDeliveryAgentByIdAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default)
        => Task.FromResult<DeliveryAgent?>(new DeliveryAgent { Id = deliveryAgentId, BranchId = Guid.NewGuid(), DeliveryFeePerOrder = 0 });

    public Task<PricingRate?> GetPricingRateAsync(Guid pricingCategoryId, Guid governorateId, CancellationToken cancellationToken = default)
        => Task.FromResult<PricingRate?>(new PricingRate { PricingCategoryId = pricingCategoryId, GovernorateId = governorateId, Size1Rate = 0, Size2Rate = 0, Size3Rate = 0, Size4Rate = 0, BranchId = Guid.NewGuid() });

    public Task<List<Order>> GetOrdersForDeliveryAgentSettlementAsync(Guid deliveryAgentId, CancellationToken cancellationToken = default)
        => Task.FromResult(DeliveryAgentSettlementOrders.Where(x => x.DeliveryAgentId == deliveryAgentId).ToList());

    public Task<List<Order>> GetOrdersAvailableForMerchantSettlementAsync(Guid merchantId, CancellationToken cancellationToken = default)
        => Task.FromResult(MerchantSettlementOrders.Where(x => x.MerchantId == merchantId).ToList());

    public Task<List<Order>> GetOrdersPendingDeliveryAgentSettlementAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(DeliveryAgentSettlementOrders.Where(x => x.DeliveredAt != null && !x.IsDeliveryAgentSettled).ToList());

    public Task<List<Order>> GetOrdersAvailableForMerchantSettlementListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(MerchantSettlementOrders.Where(x => x.DeliveredAt != null && x.IsDeliveryAgentSettled && !x.IsMerchantSettled).ToList());

    public Task<decimal> GetProfitabilitySumAsync(Guid? branchId, CancellationToken cancellationToken = default)
        => Task.FromResult(0m);
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

public class FakeReferenceDataCacheService : IReferenceDataCacheService
{
    public bool GovernorateExists { get; set; } = true;
    public bool ProblemCatalogExists { get; set; } = true;

    public Task<bool> GovernorateExistsAsync(Guid governorateId, CancellationToken cancellationToken = default)
        => Task.FromResult(GovernorateExists);

    public Task<bool> ProblemCatalogExistsAsync(Guid problemCatalogId, CancellationToken cancellationToken = default)
        => Task.FromResult(ProblemCatalogExists);

    public Task<PricingLookupDto?> GetPricingAsync(Guid pricingCategoryId, Guid areaId, CancellationToken cancellationToken = default)
        => Task.FromResult<PricingLookupDto?>(null);
}
