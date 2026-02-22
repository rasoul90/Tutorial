using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Reports;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeliverySaaS.Infrastructure.QueryServices;

public class DeliveryAgentStatementsQueryService : IDeliveryAgentStatementsQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;
    private readonly ILogger<DeliveryAgentStatementsQueryService> _logger;

    public DeliveryAgentStatementsQueryService(ApplicationDbContext dbContext, IRequestContext requestContext, ILogger<DeliveryAgentStatementsQueryService> logger)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
        _logger = logger;
    }

    public async Task<StatementResultDto<DeliveryAgentStatementSummaryDto, DeliveryAgentStatementRowDto>> GetStatementAsync(Guid deliveryAgentId, DateTime from, DateTime to, int page, int size, bool onlyUnsettled, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = size <= 0 ? 20 : Math.Min(size, 200);

        var baseQuery = ScopedOrders()
            .AsNoTracking()
            .Where(x => x.DeliveryAgentId == deliveryAgentId && x.DeliveredAt != null && x.DeliveredAt >= from && x.DeliveredAt <= to);

        var summary = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new DeliveryAgentStatementSummaryDto(
                g.Sum(x => x.DeliveredPriceWithDelivery ?? 0m),
                g.Sum(x => x.DeliveryFeeApplied ?? 0m),
                g.Sum(x => x.DeliveryAgentFeeApplied ?? 0m),
                g.Sum(x => x.CompanyNetDeliveryProfit ?? 0m),
                g.Count(x => !x.IsDeliveryAgentSettled),
                g.Where(x => !x.IsDeliveryAgentSettled).Sum(x => x.DeliveredPriceWithDelivery ?? 0m)
            ))
            .FirstOrDefaultAsync(cancellationToken)
            ?? new DeliveryAgentStatementSummaryDto(0, 0, 0, 0, 0, 0);

        var rowsQuery = onlyUnsettled ? baseQuery.Where(x => !x.IsDeliveryAgentSettled) : baseQuery;

        var rows = await rowsQuery
            .OrderByDescending(x => x.DeliveredAt)
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .Select(x => new
            {
                x.Id,
                Date = x.DeliveredAt!.Value,
                OrderNo = x.OrderNumber,
                x.DeliveredPriceWithDelivery,
                x.AmountToCollect,
                x.DeliveryFeeApplied,
                x.DeliveryAgentFeeApplied,
                x.CompanyNetDeliveryProfit,
                x.IsDeliveryAgentSettled,
                x.HasReturn
            })
            .ToListAsync(cancellationToken);

        var mapped = rows.Select(x =>
        {
            var delivered = x.DeliveredPriceWithDelivery ?? x.AmountToCollect;
            if (!x.DeliveredPriceWithDelivery.HasValue)
            {
                _logger.LogWarning("Order {OrderId} has null DeliveredPriceWithDelivery. Applied read fallback.", x.Id);
            }

            return new DeliveryAgentStatementRowDto(
                x.Date,
                x.OrderNo,
                delivered,
                x.DeliveryFeeApplied ?? 0m,
                x.DeliveryAgentFeeApplied ?? 0m,
                x.CompanyNetDeliveryProfit ?? 0m,
                x.IsDeliveryAgentSettled,
                x.HasReturn);
        }).ToList();

        return new StatementResultDto<DeliveryAgentStatementSummaryDto, DeliveryAgentStatementRowDto>(summary, mapped);
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
