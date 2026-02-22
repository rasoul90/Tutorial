using DeliverySaaS.Domain.Common.Entities;
using DeliverySaaS.Domain.Notifications.Enums;

namespace DeliverySaaS.Domain.Notifications.Entities;

public class Notification : BaseBranchEntity
{
    public Guid? TargetUserId { get; set; }
    public string? TargetRole { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string BodyAr { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string RelatedEntityType { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
}
