using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Operations.Entities;

public class ProblemCatalog : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
