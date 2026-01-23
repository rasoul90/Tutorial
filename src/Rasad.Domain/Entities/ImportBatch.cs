namespace Rasad.Domain.Entities;

public class ImportBatch
{
    public Guid Id { get; set; }
    public Guid MinistryId { get; set; }
    public Ministry? Ministry { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorFilePath { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
