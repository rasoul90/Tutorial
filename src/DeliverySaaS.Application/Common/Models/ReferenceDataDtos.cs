namespace DeliverySaaS.Application.Common.Models;

public record GovernorateLookupDto(Guid Id, string Name);
public record ProblemCatalogLookupDto(Guid Id, string Code, string NameAr);
public record PricingLookupDto(Guid Id, Guid PricingCategoryId, Guid AreaId, decimal Size1Rate, decimal Size2Rate, decimal Size3Rate, decimal Size4Rate);
