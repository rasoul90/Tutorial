using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Identity.Entities;

public class User : BaseEntity
{
    public Guid? BranchId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
