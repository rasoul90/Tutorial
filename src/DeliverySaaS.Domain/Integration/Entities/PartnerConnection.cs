using DeliverySaaS.Domain.Common.Entities;

namespace DeliverySaaS.Domain.Integration.Entities;

public class PartnerConnection : BaseBranchEntity
{
    public string PartnerName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
