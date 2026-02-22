namespace DeliverySaaS.Application.Notifications;

public interface INotificationSender
{
    Task SendPushAsync(Guid? targetUserId, string? targetRole, string titleAr, string bodyAr, CancellationToken cancellationToken = default);
}
