using DeliverySaaS.Application.Common.Models;
using DeliverySaaS.Domain.Notifications.Enums;

namespace DeliverySaaS.Application.Notifications;

public interface INotificationService
{
    Task CreateAsync(Guid? targetUserId, string? targetRole, string titleAr, string bodyAr, NotificationType type, string relatedEntityType, Guid? relatedEntityId, CancellationToken cancellationToken = default);
    Task<PagedResult<NotificationDto>> GetBranchNotificationsAsync(bool unreadOnly, int page, int size, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
