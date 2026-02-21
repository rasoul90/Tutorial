using System.Text.Json;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Accounting.Entities;

namespace DeliverySaaS.Application.Accounting;

public class AccountingService : IAccountingService
{
    private readonly IAccountingRepository _accountingRepository;
    private readonly IRequestContext _requestContext;

    public AccountingService(IAccountingRepository accountingRepository, IRequestContext requestContext)
    {
        _accountingRepository = accountingRepository;
        _requestContext = requestContext;
    }

    public async Task<Guid> CreateMerchantSettlementRequestAsync(Guid merchantId, decimal amount, CancellationToken cancellationToken = default)
    {
        var entity = new MerchantSettlementRequest
        {
            MerchantId = merchantId,
            Amount = amount,
            Status = "Pending",
            BranchId = RequiredBranchId()
        };

        await _accountingRepository.AddSettlementRequestAsync(entity, cancellationToken);
        await AddAuditAsync("MerchantSettlementRequest", entity.Id, "Create", entity, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
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

        await _accountingRepository.AddDeliveryReconciliationAsync(entity, cancellationToken);
        await AddAuditAsync("DeliveryReconciliation", entity.Id, "Record", entity, cancellationToken);
        await _accountingRepository.SaveChangesAsync(cancellationToken);
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
