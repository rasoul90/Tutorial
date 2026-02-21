namespace DeliverySaaS.Application.Common.Interfaces;

public interface IRequestContext
{
    Guid? TenantId { get; }
    Guid? BranchId { get; }
    bool IsCompanyAdmin { get; }
    bool IsSaasAdmin { get; }
    IReadOnlyCollection<string> Roles { get; }
}
