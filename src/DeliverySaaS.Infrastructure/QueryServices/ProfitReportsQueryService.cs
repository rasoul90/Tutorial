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
        var scopedOrders = ScopedOrders().AsNoTracking()
            .Where(o => o.DeliveredAt != null)
            .Where(o => o.DeliveredAt >= from)
            .Where(o => o.DeliveredAt <= to);

        var rows = await scopedOrders
            .Select(o => new
            {
                o.GovernorateId,
                o.PricingCategoryId,
                o.OrderSize,
                DeliveryFee = o.DeliveryFeeApplied ?? 0m,
                AgentFee = o.DeliveryAgentFeeApplied ?? 0m,
                Profit = o.CompanyNetDeliveryProfit ?? 0m
            })
            .ToListAsync(cancellationToken);

        var governorates = await _dbContext.Governorates.AsNoTracking()
            .Select(g => new { g.Id, g.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var pricingCategories = await _dbContext.PricingCategories.AsNoTracking()
            .Select(p => new { p.Id, p.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var enriched = rows.Select(r => new
        {
            Governorate = r.GovernorateId.HasValue && governorates.TryGetValue(r.GovernorateId.Value, out var govName) ? govName : "-",
            Size = r.OrderSize.HasValue ? r.OrderSize.Value.ToString() : "-",
            PricingCategory = r.PricingCategoryId.HasValue && pricingCategories.TryGetValue(r.PricingCategoryId.Value, out var catName) ? catName : "-",
            r.DeliveryFee,
            r.AgentFee,
            r.Profit
        }).ToList();

        string BuildGroupKey(dynamic r) => groupBy switch
        {
            "governorate" => r.Governorate,
            "size" => r.Size,
            "pricingCategory" => r.PricingCategory,
            "governorate_size" => $"{r.Governorate}|{r.Size}",
            "pricing_size" => $"{r.PricingCategory}|{r.Size}",
            _ => r.Governorate
        };

        return enriched
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
        var rows = await ScopedOrders().AsNoTracking()
            .Where(o => o.DeliveredAt != null)
            .Where(o => o.DeliveredAt >= from)
            .Where(o => o.DeliveredAt <= to)
            .Where(o => o.DeliveryAgentId != null)
            .Join(_dbContext.DeliveryAgents.AsNoTracking(),
                o => o.DeliveryAgentId,
                d => d.Id,
                (o, d) => new { Order = o, AgentName = d.Name })
            .GroupBy(x => x.AgentName)
            .Select(g => new DeliveryAgentPerformanceRowDto(
                g.Key,
                g.Count(),
                g.Count(x => x.Order.HasReturn),
                g.Count(x => x.Order.HasProblem),
                g.Sum(x => x.Order.DeliveryAgentFeeApplied ?? 0m),
                g.Sum(x => x.Order.CompanyNetDeliveryProfit ?? 0m),
                g.Count(x => !x.Order.IsDeliveryAgentSettled)))
            .OrderByDescending(x => x.DeliveredCount)
            .ToListAsync(cancellationToken);

        return rows;
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
