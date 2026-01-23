namespace Rasad.Domain.Entities;

public class Section
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
