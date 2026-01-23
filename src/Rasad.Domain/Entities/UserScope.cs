namespace Rasad.Domain.Entities;

public class UserScope
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
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
    public bool CanImportExcel { get; set; }
    public bool CanExportExcel { get; set; }
    public bool CanViewNationalIdFull { get; set; }
}
