using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Reports;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.QueryServices;

public class ProfitReportsQueryService : IProfitReportsQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public ProfitReportsQueryService(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public async Task<IReadOnlyList<ProfitBreakdownRowDto>> GetProfitBreakdownAsync(DateTime from, DateTime to, string groupBy, CancellationToken cancellationToken = default)
    {
        var rows = await (
            from o in ScopedOrders().AsNoTracking()
            where o.DeliveredAt != null && o.DeliveredAt >= from && o.DeliveredAt <= to
            join g in _dbContext.Governorates.AsNoTracking() on o.GovernorateId equals g.Id into gg
            from gov in gg.DefaultIfEmpty()
            join pc in _dbContext.PricingCategories.AsNoTracking() on o.PricingCategoryId equals pc.Id into pp
            from pricing in pp.DefaultIfEmpty()
            select new
            {
                Governorate = gov != null ? gov.Name : "-",
                Size = o.OrderSize.HasValue ? o.OrderSize.Value.ToString() : "-",
                PricingCategory = pricing != null ? pricing.Name : "-",
                DeliveryFee = o.DeliveryFeeApplied ?? 0m,
                AgentFee = o.DeliveryAgentFeeApplied ?? 0m,
                Profit = o.CompanyNetDeliveryProfit ?? 0m
            })
            .ToListAsync(cancellationToken);

        string BuildGroupKey(dynamic r) => groupBy switch
        {
            "governorate" => r.Governorate,
            "size" => r.Size,
            "pricingCategory" => r.PricingCategory,
            "governorate_size" => $"{r.Governorate}|{r.Size}",
            "pricing_size" => $"{r.PricingCategory}|{r.Size}",
            _ => r.Governorate
        };

        return rows
            .GroupBy(BuildGroupKey)
            .Select(g =>
            {
                var first = g.First();
                var includeGov = groupBy.Contains("governorate", StringComparison.OrdinalIgnoreCase);
                var includeSize = groupBy.Contains("size", StringComparison.OrdinalIgnoreCase);
                var includePricing = groupBy.Contains("pricing", StringComparison.OrdinalIgnoreCase)
                                     || groupBy.Equals("pricingCategory", StringComparison.OrdinalIgnoreCase);

                return new ProfitBreakdownRowDto(
                    includeGov ? first.Governorate : "-",
                    includeSize ? first.Size : "-",
                    includePricing ? first.PricingCategory : "-",
                    g.Count(),
                    g.Sum(x => x.DeliveryFee),
                    g.Sum(x => x.AgentFee),
                    g.Sum(x => x.Profit));
            })
            .OrderByDescending(x => x.TotalCompanyNetProfit)
            .ToList();
    }

    public async Task<IReadOnlyList<DeliveryAgentPerformanceRowDto>> GetDeliveryAgentPerformanceAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await (
            from o in ScopedOrders().AsNoTracking()
            where o.DeliveredAt != null && o.DeliveredAt >= from && o.DeliveredAt <= to && o.DeliveryAgentId != null
            join d in _dbContext.DeliveryAgents.AsNoTracking() on o.DeliveryAgentId equals d.Id
            group o by new { d.Id, d.Name } into g
            select new DeliveryAgentPerformanceRowDto(
                g.Key.Name,
                g.Count(),
                g.Count(x => x.HasReturn),
                g.Count(x => x.HasProblem),
                g.Sum(x => x.DeliveryAgentFeeApplied ?? 0m),
                g.Sum(x => x.CompanyNetDeliveryProfit ?? 0m),
                g.Count(x => !x.IsDeliveryAgentSettled)))
            .OrderByDescending(x => x.DeliveredCount)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Domain.Operations.Entities.Order> ScopedOrders()
    {
        if (_requestContext.IsSaasAdmin)
        {
            return _dbContext.Orders;
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
