namespace DeliverySaaS.Application.Notifications;

public class NoOpNotificationSender : INotificationSender
{
    public Task SendPushAsync(Guid? targetUserId, string? targetRole, string titleAr, string bodyAr, CancellationToken cancellationToken = default)
    {
        // TODO: integrate external push provider (e.g., FCM) in future phases.
        return Task.CompletedTask;
    }
}
