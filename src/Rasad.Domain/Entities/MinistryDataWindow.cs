namespace Rasad.Domain.Entities;

public class MinistryDataWindow
{
    public Guid Id { get; set; }
    public Guid MinistryId { get; set; }
    public Ministry? Ministry { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsActive { get; set; }
    public string AfterCloseMessageAr { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
