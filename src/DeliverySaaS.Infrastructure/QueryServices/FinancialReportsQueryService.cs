using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Reports;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.QueryServices;

public class FinancialReportsQueryService : IFinancialReportsQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public FinancialReportsQueryService(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public Task<BranchFinancialSummaryDto> GetBranchFinancialSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        if (!_requestContext.BranchId.HasValue)
        {
            throw new InvalidOperationException("BranchId is required.");
        }

        return BuildSummaryAsync(ScopedOrders().Where(x => x.BranchId == _requestContext.BranchId.Value), from, to, cancellationToken);
    }

    public Task<BranchFinancialSummaryDto> GetCompanyFinancialSummaryAsync(DateTime from, DateTime to, Guid? branchId = null, CancellationToken cancellationToken = default)
    {
        var scoped = ScopedOrders();
        if (branchId.HasValue)
        {
            scoped = scoped.Where(x => x.BranchId == branchId.Value);
        }

        return BuildSummaryAsync(scoped, from, to, cancellationToken);
    }

    private static async Task<BranchFinancialSummaryDto> BuildSummaryAsync(IQueryable<Domain.Operations.Entities.Order> orders, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var query = orders
            .AsNoTracking()
            .Where(x => x.DeliveredAt != null && x.DeliveredAt >= from && x.DeliveredAt <= to);

        var data = await query
            .GroupBy(_ => 1)
            .Select(g => new BranchFinancialSummaryDto(
                g.Count(),
                g.Sum(x => x.DeliveredPriceWithDelivery ?? 0m),
                g.Sum(x => x.DeliveryFeeApplied ?? 0m),
                g.Sum(x => x.DeliveryAgentFeeApplied ?? 0m),
                g.Sum(x => x.CompanyNetDeliveryProfit ?? 0m),
                g.Sum(x => x.MerchantDueAmount ?? 0m),
                g.Count(x => !x.IsDeliveryAgentSettled),
                g.Where(x => !x.IsDeliveryAgentSettled).Sum(x => x.DeliveredPriceWithDelivery ?? 0m),
                g.Count(x => x.IsDeliveryAgentSettled && !x.IsMerchantSettled),
                g.Where(x => x.IsDeliveryAgentSettled && !x.IsMerchantSettled).Sum(x => x.MerchantDueAmount ?? 0m)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return data ?? new BranchFinancialSummaryDto(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    private IQueryable<Domain.Operations.Entities.Order> ScopedOrders()
    {
        if (_requestContext.IsSaasAdmin)
        {
            return _dbContext.Orders.AsQueryable();
        }

        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        var query = _dbContext.Orders.Where(x => x.TenantId == tenantId);

        if (_requestContext.IsCompanyAdmin)
        {
            return query;
        }

        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return query.Where(x => x.BranchId == branchId);
    }
}
