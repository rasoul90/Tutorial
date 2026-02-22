using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Reports;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeliverySaaS.Infrastructure.QueryServices;

public class MerchantStatementsQueryService : IMerchantStatementsQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;
    private readonly ILogger<MerchantStatementsQueryService> _logger;

    public MerchantStatementsQueryService(ApplicationDbContext dbContext, IRequestContext requestContext, ILogger<MerchantStatementsQueryService> logger)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
        _logger = logger;
    }

    public async Task<StatementResultDto<MerchantStatementSummaryDto, MerchantStatementRowDto>> GetStatementAsync(Guid merchantId, DateTime from, DateTime to, int page, int size, bool onlyReadyForSettlement, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = size <= 0 ? 20 : Math.Min(size, 200);

        var baseQuery = ScopedOrders()
            .AsNoTracking()
            .Where(x => x.MerchantId == merchantId && x.DeliveredAt != null && x.DeliveredAt >= from && x.DeliveredAt <= to);

        var summary = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new MerchantStatementSummaryDto(
                g.Sum(x => x.DeliveredPriceWithDelivery ?? 0m),
                g.Sum(x => x.DeliveryFeeApplied ?? 0m),
                g.Sum(x => x.MerchantDueAmount ?? 0m),
                g.Where(x => x.IsMerchantSettled).Sum(x => x.MerchantDueAmount ?? 0m),
                g.Where(x => !x.IsMerchantSettled && x.IsDeliveryAgentSettled).Sum(x => x.MerchantDueAmount ?? 0m),
                g.Where(x => !x.IsDeliveryAgentSettled).Sum(x => x.MerchantDueAmount ?? 0m)
            ))
            .FirstOrDefaultAsync(cancellationToken)
            ?? new MerchantStatementSummaryDto(0, 0, 0, 0, 0, 0);

        var rowsQuery = baseQuery;
        if (onlyReadyForSettlement)
        {
            rowsQuery = rowsQuery.Where(x => x.IsDeliveryAgentSettled);
        }

        var rows = await (from o in rowsQuery
                          join g in _dbContext.Governorates.AsNoTracking() on o.GovernorateId equals g.Id into gg
                          from gov in gg.DefaultIfEmpty()
                          orderby o.DeliveredAt descending
                          select new
                          {
                              o.Id,
                              Date = o.DeliveredAt!.Value,
                              OrderNo = o.OrderNumber,
                              o.CustomerName,
                              Governorate = gov != null ? gov.Name : "-",
                              Size = o.OrderSize.HasValue ? o.OrderSize.Value.ToString() : "-",
                              o.DeliveredPriceWithDelivery,
                              o.AmountToCollect,
                              o.DeliveryFeeApplied,
                              o.MerchantDueAmount,
                              o.IsDeliveryAgentSettled,
                              o.IsMerchantSettled,
                              o.HasReturn,
                              o.InternalNote
                          })
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .ToListAsync(cancellationToken);

        var mapped = rows.Select(x =>
        {
            var delivered = x.DeliveredPriceWithDelivery ?? x.AmountToCollect;
            if (!x.DeliveredPriceWithDelivery.HasValue)
            {
                _logger.LogWarning("Order {OrderId} has null DeliveredPriceWithDelivery. Applied read fallback.", x.Id);
            }

            return new MerchantStatementRowDto(
                x.Date,
                x.OrderNo,
                x.CustomerName,
                x.Governorate,
                x.Size,
                delivered,
                x.DeliveryFeeApplied ?? 0m,
                x.MerchantDueAmount ?? 0m,
                x.IsDeliveryAgentSettled,
                x.IsMerchantSettled,
                x.HasReturn,
                x.InternalNote);
        }).ToList();

        return new StatementResultDto<MerchantStatementSummaryDto, MerchantStatementRowDto>(summary, mapped);
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
