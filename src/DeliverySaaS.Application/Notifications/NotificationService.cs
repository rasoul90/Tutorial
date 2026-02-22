using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Common.Models;
using DeliverySaaS.Domain.Notifications.Entities;
using DeliverySaaS.Domain.Notifications.Enums;

namespace DeliverySaaS.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IRequestContext _requestContext;
    private readonly INotificationSender _sender;

    public NotificationService(INotificationRepository repository, IRequestContext requestContext, INotificationSender sender)
    {
        _repository = repository;
        _requestContext = requestContext;
        _sender = sender;
    }

    public async Task CreateAsync(Guid? targetUserId, string? targetRole, string titleAr, string bodyAr, NotificationType type, string relatedEntityType, Guid? relatedEntityId, CancellationToken cancellationToken = default)
    {
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
        var entity = new Notification
        {
            TargetUserId = targetUserId,
            TargetRole = targetRole,
            TitleAr = titleAr,
            BodyAr = bodyAr,
            Type = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            BranchId = branchId
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _sender.SendPushAsync(targetUserId, targetRole, titleAr, bodyAr, cancellationToken);
    }

    public async Task<PagedResult<NotificationDto>> GetBranchNotificationsAsync(bool unreadOnly, int page, int size, CancellationToken cancellationToken = default)
    {
        var rows = await _repository.GetBranchNotificationsAsync(unreadOnly, page, size, cancellationToken);
        return new PagedResult<NotificationDto>(rows.Select(x => new NotificationDto(x.Id, x.TitleAr, x.BodyAr, x.Type.ToString(), x.RelatedEntityType, x.RelatedEntityId, x.IsRead, x.CreatedAt)).ToList(), page, size);
    }

    public async Task MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(notificationId, cancellationToken) ?? throw new InvalidOperationException("Notification not found.");
        entity.IsRead = true;
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
