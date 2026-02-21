using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Common.Models;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DeliverySaaS.Infrastructure.Caching;

public class ReferenceDataCacheService : IReferenceDataCacheService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly ApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;
    private readonly IMemoryCache _memoryCache;

    public ReferenceDataCacheService(ApplicationDbContext dbContext, IRequestContext requestContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
        _memoryCache = memoryCache;
    }

    public async Task<bool> GovernorateExistsAsync(Guid governorateId, CancellationToken cancellationToken = default)
    {
        var tenantId = RequiredTenant();
        var cacheKey = $"ref:g:{tenantId}";

        var items = await _memoryCache.GetOrCreateAsync(cacheKey, async e =>
        {
            e.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _dbContext.Governorates
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .Select(x => new GovernorateLookupDto(x.Id, x.Name))
                .ToListAsync(cancellationToken);
        }) ?? [];

        return items.Any(x => x.Id == governorateId);
    }

    public async Task<bool> ProblemCatalogExistsAsync(Guid problemCatalogId, CancellationToken cancellationToken = default)
    {
        var tenantId = RequiredTenant();
        var branchId = RequiredBranch();
        var cacheKey = $"ref:pc:{tenantId}:{branchId}";

        var items = await _memoryCache.GetOrCreateAsync(cacheKey, async e =>
        {
            e.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _dbContext.ProblemCatalogs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .Select(x => new ProblemCatalogLookupDto(x.Id, x.Code, x.NameAr))
                .ToListAsync(cancellationToken);
        }) ?? [];

        return items.Any(x => x.Id == problemCatalogId);
    }

    public async Task<PricingLookupDto?> GetPricingAsync(Guid pricingCategoryId, Guid areaId, CancellationToken cancellationToken = default)
    {
        var tenantId = RequiredTenant();
        var branchId = RequiredBranch();
        var cacheKey = $"ref:pr:{tenantId}:{branchId}";

        var items = await _memoryCache.GetOrCreateAsync(cacheKey, async e =>
        {
            e.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _dbContext.PricingRates
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.BranchId == branchId)
                .Select(x => new PricingLookupDto(x.Id, x.PricingCategoryId, x.AreaId, x.Size1Rate, x.Size2Rate, x.Size3Rate, x.Size4Rate))
                .ToListAsync(cancellationToken);
        }) ?? [];

        return items.FirstOrDefault(x => x.PricingCategoryId == pricingCategoryId && x.AreaId == areaId);
    }

    private Guid RequiredTenant() => _requestContext.TenantId ?? throw new InvalidOperationException("TenantId is required.");
    private Guid RequiredBranch() => _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
}
