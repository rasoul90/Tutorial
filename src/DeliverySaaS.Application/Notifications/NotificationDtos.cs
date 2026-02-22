namespace DeliverySaaS.Application.Notifications;

public record NotificationDto(Guid Id, string TitleAr, string BodyAr, string Type, string RelatedEntityType, Guid? RelatedEntityId, bool IsRead, DateTime CreatedAt);
