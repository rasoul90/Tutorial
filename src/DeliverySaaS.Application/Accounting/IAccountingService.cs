using DeliverySaaS.Domain.Operations.Entities;

namespace DeliverySaaS.Application.Accounting;

public interface IAccountingService
{
    Task<Guid> CreateMerchantSettlementRequestAsync(Guid merchantId, decimal amount, CancellationToken cancellationToken = default);
    Task<Guid> GenerateMerchantInvoiceAsync(Guid merchantId, decimal totalAmount, CancellationToken cancellationToken = default);
    Task<Guid> ApproveSettlementAndGenerateInvoiceAsync(Guid settlementRequestId, CancellationToken cancellationToken = default);
    Task<Guid> RecordDeliveryReconciliationAsync(Guid deliveryAgentId, decimal collectedAmount, decimal deliveredAmount, CancellationToken cancellationToken = default);
    Task<Guid> CreatePayrollAsync(Guid userId, decimal amount, DateTime payrollDate, CancellationToken cancellationToken = default);
    Task<Guid> CreateExpenseAsync(string category, decimal amount, string? notes, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrdersPendingDeliveryAgentSettlementAsync(CancellationToken cancellationToken = default);
    Task<List<Order>> GetOrdersAvailableForMerchantSettlementAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetCompanyNetDeliveryProfitAsync(Guid? branchId = null, CancellationToken cancellationToken = default);
}
