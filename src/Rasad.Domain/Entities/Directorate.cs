namespace Rasad.Domain.Entities;

public class Directorate
{
    public Guid Id { get; set; }
    public Guid MinistryId { get; set; }
    public Ministry? Ministry { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}
