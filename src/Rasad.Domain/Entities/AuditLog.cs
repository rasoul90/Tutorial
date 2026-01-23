namespace Rasad.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
}
