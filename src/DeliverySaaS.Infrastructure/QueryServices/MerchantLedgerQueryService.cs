using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Reports;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.QueryServices;

public class MerchantLedgerQueryService : IMerchantLedgerQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public MerchantLedgerQueryService(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public async Task<MerchantLedgerResultDto> GetLedgerAsync(Guid merchantId, DateTime from, DateTime to, int page, int size, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = size <= 0 ? 20 : Math.Min(size, 200);

        var ordersBase = ScopedOrders().AsNoTracking().Where(x => x.MerchantId == merchantId && x.DeliveredAt != null);
        var paymentsBase = ScopedPayments().AsNoTracking().Where(x => x.MerchantId == merchantId);

        var openingDebit = await ordersBase.Where(x => x.DeliveredAt < from).SumAsync(x => x.MerchantDueAmount ?? 0m, cancellationToken);
        var openingCredit = await paymentsBase.Where(x => x.PaymentDate < from).SumAsync(x => x.Amount, cancellationToken);
        var openingBalance = openingDebit - openingCredit;

        var orderRows = await ordersBase.Where(x => x.DeliveredAt >= from && x.DeliveredAt <= to)
            .Select(x => new { Date = x.DeliveredAt!.Value, Type = "OrderDue", Description = $"مستحق طلب {x.OrderNumber}", Debit = x.MerchantDueAmount ?? 0m, Credit = 0m })
            .ToListAsync(cancellationToken);
        var paymentRows = await paymentsBase.Where(x => x.PaymentDate >= from && x.PaymentDate <= to)
            .Select(x => new { Date = x.PaymentDate, Type = "Payment", Description = $"دفعة ({x.Method})", Debit = 0m, Credit = x.Amount })
            .ToListAsync(cancellationToken);

        var merged = orderRows.Cast<dynamic>().Concat(paymentRows)
            .OrderBy(x => (DateTime)x.Date)
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .ToList();

        decimal balance = openingBalance;
        var rows = new List<MerchantLedgerRowDto>();
        foreach (var row in merged)
        {
            balance += (decimal)row.Debit - (decimal)row.Credit;
            rows.Add(new MerchantLedgerRowDto((DateTime)row.Date, (string)row.Type, (string)row.Description, (decimal)row.Debit, (decimal)row.Credit, balance));
        }

        var totalDebit = rows.Sum(x => x.Debit);
        var totalCredit = rows.Sum(x => x.Credit);
        var closingBalance = openingBalance + totalDebit - totalCredit;

        return new MerchantLedgerResultDto(openingBalance, rows, new MerchantLedgerSummaryDto(totalDebit, totalCredit, closingBalance));
    }

    private IQueryable<Domain.Operations.Entities.Order> ScopedOrders()
    {
        if (_requestContext.IsSaasAdmin) return _dbContext.Orders;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        var query = _dbContext.Orders.Where(x => x.TenantId == tenantId);
        if (_requestContext.IsCompanyAdmin) return query;
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return query.Where(x => x.BranchId == branchId);
    }

    private IQueryable<Domain.Accounting.Entities.MerchantPayment> ScopedPayments()
    {
        if (_requestContext.IsSaasAdmin) return _dbContext.MerchantPayments;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        var query = _dbContext.MerchantPayments.Where(x => x.TenantId == tenantId);
        if (_requestContext.IsCompanyAdmin) return query;
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return query.Where(x => x.BranchId == branchId);
    }
}
