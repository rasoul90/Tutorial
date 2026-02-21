using DeliverySaaS.Application.Common.Interfaces;

namespace DeliverySaaS.API.Security;

public class RequestContext : IRequestContext
{
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
}
