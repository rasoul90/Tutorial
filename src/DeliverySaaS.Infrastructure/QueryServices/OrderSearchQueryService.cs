using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Common.Models;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.QueryServices;

public class OrderSearchQueryService : IOrderSearchQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public OrderSearchQueryService(ApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public async Task<PagedResult<OrderSearchRowDto>> SearchAsync(bool includeArchive, int page, int size, CancellationToken cancellationToken = default)
    {
        var p = page <= 0 ? 1 : page;
        var s = size <= 0 ? 20 : Math.Min(size, 200);
        var live = await Scoped(_dbContext.Orders.AsNoTracking())
            .OrderByDescending(x => x.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(x => new OrderSearchRowDto(x.Id, x.OrderNumber, x.DeliveredAt, false))
            .ToListAsync(cancellationToken);

        if (!includeArchive)
        {
            return new PagedResult<OrderSearchRowDto>(live, p, s);
        }

        var archive = await Scoped(_dbContext.OrdersArchive.AsNoTracking())
            .OrderByDescending(x => x.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(x => new OrderSearchRowDto(x.OriginalOrderId, x.OrderNumber, x.DeliveredAt, true))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderSearchRowDto>(live.Concat(archive).OrderByDescending(x => x.DeliveredAt).Take(s).ToList(), p, s);
    }

    private IQueryable<T> Scoped<T>(IQueryable<T> q) where T : class
    {
        if (_requestContext.IsSaasAdmin) return q;
        var tenantId = _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
        q = q.Where(x => EF.Property<Guid>(x, "TenantId") == tenantId);
        if (_requestContext.IsCompanyAdmin) return q;
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        return q.Where(x => EF.Property<Guid>(x, "BranchId") == branchId);
    }
}
