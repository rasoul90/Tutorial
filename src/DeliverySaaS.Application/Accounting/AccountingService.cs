using System.Text.Json;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Application.Accounting;

public class AccountingService : IAccountingService
{
    private readonly IAccountingRepository _accountingRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestContext _requestContext;

    public AccountingService(IAccountingRepository accountingRepository, IOrderRepository orderRepository, IRequestContext requestContext)
    {
        _accountingRepository = accountingRepository;
        _orderRepository = orderRepository;
        _requestContext = requestContext;
    }

    public async Task<Guid> CreateMerchantSettlementRequestAsync(Guid merchantId, decimal amount, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrdersAvailableForMerchantSettlementAsync(merchantId, cancellationToken);
        if (orders.Count == 0)
        {
            throw new InvalidOperationException("No orders available for merchant settlement.");
        }

        var computedAmount = orders.Sum(x => x.MerchantDueAmount ?? 0m);

        foreach (var order in orders)
        {
            order.IsMerchantSettled = true;
            order.MerchantSettledAt = DateTime.UtcNow;

            await _orderRepository.AddOrderEventAsync(new Domain.Operations.Entities.OrderEvent
            {
                OrderId = order.Id,
                EventType = order.HasReturn ? "MerchantSettledWithReturn" : "MerchantSettled",
                Notes = JsonSerializer.Serialize(new { order.MerchantDueAmount, order.MerchantSettledAt, order.HasReturn }),
                EventAt = DateTime.UtcNow,
                BranchId = RequiredBranchId()
            }, cancellationToken);
        }

        var entity = new MerchantSettlementRequest
        {
            MerchantId = merchantId,
            Amount = computedAmount <= 0 ? amount : computedAmount,
            Status = "Pending",
            BranchId = RequiredBranchId()
        };

        await _accountingRepository.AddSettlementRequestAsync(entity, cancellationToken);
        await AddAuditAsync("MerchantSettlementRequest", entity.Id, "Create", entity, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<Guid> GenerateMerchantInvoiceAsync(Guid merchantId, decimal totalAmount, CancellationToken cancellationToken = default)
    {
        var invoice = new MerchantInvoice
        {
            MerchantId = merchantId,
            TotalAmount = totalAmount,
            InvoiceDate = DateTime.UtcNow,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            BranchId = RequiredBranchId()
        };

        await _accountingRepository.AddInvoiceAsync(invoice, cancellationToken);
        await AddAuditAsync("MerchantInvoice", invoice.Id, "Generate", invoice, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }

    public async Task<Guid> ApproveSettlementAndGenerateInvoiceAsync(Guid settlementRequestId, CancellationToken cancellationToken = default)
    {
        var settlement = await _accountingRepository.GetSettlementRequestByIdAsync(settlementRequestId, cancellationToken)
            ?? throw new InvalidOperationException("Settlement request not found.");

        settlement.Status = "Approved";

        var invoice = new MerchantInvoice
        {
            MerchantId = settlement.MerchantId,
            TotalAmount = settlement.Amount,
            InvoiceDate = DateTime.UtcNow,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            BranchId = RequiredBranchId()
        };

        await _accountingRepository.AddInvoiceAsync(invoice, cancellationToken);
        await AddAuditAsync("MerchantSettlementRequest", settlement.Id, "Approve", settlement, cancellationToken);
        await AddAuditAsync("MerchantInvoice", invoice.Id, "GenerateFromSettlement", invoice, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }

    public async Task<Guid> RecordDeliveryReconciliationAsync(Guid deliveryAgentId, decimal collectedAmount, decimal deliveredAmount, CancellationToken cancellationToken = default)
    {
        var entity = new DeliveryReconciliation
        {
            DeliveryAgentId = deliveryAgentId,
            CollectedAmount = collectedAmount,
            DeliveredAmount = deliveredAmount,
            ReconciledAt = DateTime.UtcNow,
            BranchId = RequiredBranchId()
        };

        var orders = await _orderRepository.GetOrdersForDeliveryAgentSettlementAsync(deliveryAgentId, cancellationToken);

        foreach (var order in orders)
        {
            order.IsDeliveryAgentSettled = true;
            order.DeliveryAgentSettledAt = DateTime.UtcNow;
            order.MerchantSettlementStatus = order.MerchantRemainingAmount > 0 ? (order.MerchantPaidAmount > 0 ? MerchantSettlementStatus.PartiallyPaid : MerchantSettlementStatus.Ready) : MerchantSettlementStatus.Paid;

            await _orderRepository.AddOrderEventAsync(new Domain.Operations.Entities.OrderEvent
            {
                OrderId = order.Id,
                EventType = "DeliveryAgentSettled",
                Notes = JsonSerializer.Serialize(new { order.DeliveryAgentSettledAt }),
                EventAt = DateTime.UtcNow,
                BranchId = RequiredBranchId()
            }, cancellationToken);

            if (order.HasReturn)
            {
                order.State = OperationalState.ReturnSortingHub;
                await _orderRepository.AddOrderEventAsync(new Domain.Operations.Entities.OrderEvent
                {
                    OrderId = order.Id,
                    EventType = "ReturnEnteredHubAfterDeliverySettlement",
                    Notes = "Returned item entered hub after delivery agent settlement.",
                    EventAt = DateTime.UtcNow,
                    BranchId = RequiredBranchId()
                }, cancellationToken);
            }
        }

        await _accountingRepository.AddDeliveryReconciliationAsync(entity, cancellationToken);
        await AddAuditAsync("DeliveryReconciliation", entity.Id, "Record", entity, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<Guid> CreatePayrollAsync(Guid userId, decimal amount, DateTime payrollDate, CancellationToken cancellationToken = default)
    {
        var entity = new Payroll
        {
            UserId = userId,
            Amount = amount,
            PayrollDate = payrollDate,
            BranchId = RequiredBranchId()
        };

        await _accountingRepository.AddPayrollAsync(entity, cancellationToken);
        await AddAuditAsync("Payroll", entity.Id, "Create", entity, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<Guid> CreateExpenseAsync(string category, decimal amount, string? notes, CancellationToken cancellationToken = default)
    {
        var entity = new Expense
        {
            Category = category,
            Amount = amount,
            Notes = notes,
            BranchId = RequiredBranchId()
        };

        await _accountingRepository.AddExpenseAsync(entity, cancellationToken);
        await AddAuditAsync("Expense", entity.Id, "Create", entity, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public Task<List<Order>> GetOrdersPendingDeliveryAgentSettlementAsync(CancellationToken cancellationToken = default)
        => _orderRepository.GetOrdersPendingDeliveryAgentSettlementAsync(cancellationToken);

    public Task<List<Order>> GetOrdersAvailableForMerchantSettlementAsync(CancellationToken cancellationToken = default)
        => _orderRepository.GetOrdersAvailableForMerchantSettlementListAsync(cancellationToken);

    public Task<decimal> GetCompanyNetDeliveryProfitAsync(Guid? branchId = null, CancellationToken cancellationToken = default)
        => _orderRepository.GetProfitabilitySumAsync(branchId, cancellationToken);

    private async Task AddAuditAsync(string entityName, Guid entityId, string operation, object payload, CancellationToken cancellationToken)
    {
        var audit = new AuditEntry
        {
            EntityName = entityName,
            EntityId = entityId,
            Operation = operation,
            PayloadJson = JsonSerializer.Serialize(payload),
            BranchId = RequiredBranchId()
        };

        await _accountingRepository.AddAuditEntryAsync(audit, cancellationToken);
    }

    private Guid RequiredBranchId() => _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
}
