namespace DeliverySaaS.Application.Auditing;

public record AuditLogDto(Guid Id, string Action, string EntityType, string EntityId, string SummaryAr, string DiffJson, DateTime CreatedAt);
