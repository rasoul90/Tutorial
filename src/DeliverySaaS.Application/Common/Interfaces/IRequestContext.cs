namespace DeliverySaaS.Application.Common.Interfaces;

public interface IRequestContext
{
    Guid? TenantId { get; }
    Guid? BranchId { get; }
}
