namespace Rasad.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string JobNumber { get; set; } = string.Empty;
    public byte[] NationalIdEncrypted { get; set; } = Array.Empty<byte>();
    public byte[] NationalIdHash { get; set; } = Array.Empty<byte>();
    public Guid MinistryId { get; set; }
    public Ministry? Ministry { get; set; }
    public Guid DirectorateId { get; set; }
    public Directorate? Directorate { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? SectionId { get; set; }
    public Section? Section { get; set; }
    public Guid? UnitId { get; set; }
    public Unit? Unit { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedByUserId { get; set; }
    public string VerifiedSource { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string UpdatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }
}
