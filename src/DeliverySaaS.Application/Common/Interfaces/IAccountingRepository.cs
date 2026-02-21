using DeliverySaaS.Domain.Accounting.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IAccountingRepository
{
    Task<MerchantSettlementRequest?> GetSettlementRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddSettlementRequestAsync(MerchantSettlementRequest settlementRequest, CancellationToken cancellationToken = default);
    Task AddInvoiceAsync(MerchantInvoice invoice, CancellationToken cancellationToken = default);
    Task AddDeliveryReconciliationAsync(DeliveryReconciliation reconciliation, CancellationToken cancellationToken = default);
    Task AddPayrollAsync(Payroll payroll, CancellationToken cancellationToken = default);
    Task AddExpenseAsync(Expense expense, CancellationToken cancellationToken = default);
    Task AddAuditEntryAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
