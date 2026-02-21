using DeliverySaaS.Application.Common.Models;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IReferenceDataCacheService
{
    Task<bool> GovernorateExistsAsync(Guid governorateId, CancellationToken cancellationToken = default);
    Task<bool> ProblemCatalogExistsAsync(Guid problemCatalogId, CancellationToken cancellationToken = default);
    Task<PricingLookupDto?> GetPricingAsync(Guid pricingCategoryId, Guid areaId, CancellationToken cancellationToken = default);
}
