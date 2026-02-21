using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Repositories;

public class AccountingRepository : IAccountingRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AccountingRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MerchantSettlementRequest?> GetSettlementRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.MerchantSettlementRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddSettlementRequestAsync(MerchantSettlementRequest settlementRequest, CancellationToken cancellationToken = default)
        => _dbContext.MerchantSettlementRequests.AddAsync(settlementRequest, cancellationToken).AsTask();

    public Task AddInvoiceAsync(MerchantInvoice invoice, CancellationToken cancellationToken = default)
        => _dbContext.MerchantInvoices.AddAsync(invoice, cancellationToken).AsTask();

    public Task AddDeliveryReconciliationAsync(DeliveryReconciliation reconciliation, CancellationToken cancellationToken = default)
        => _dbContext.DeliveryReconciliations.AddAsync(reconciliation, cancellationToken).AsTask();

    public Task AddPayrollAsync(Payroll payroll, CancellationToken cancellationToken = default)
        => _dbContext.Payroll.AddAsync(payroll, cancellationToken).AsTask();

    public Task AddExpenseAsync(Expense expense, CancellationToken cancellationToken = default)
        => _dbContext.Expenses.AddAsync(expense, cancellationToken).AsTask();

    public Task AddAuditEntryAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default)
        => _dbContext.Set<AuditEntry>().AddAsync(auditEntry, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
