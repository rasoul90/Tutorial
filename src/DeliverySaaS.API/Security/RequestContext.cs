using DeliverySaaS.Application.Common.Interfaces;

namespace DeliverySaaS.API.Security;

public class RequestContext : IRequestContext
{
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsCompanyAdmin { get; set; }
    public bool IsSaasAdmin { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
