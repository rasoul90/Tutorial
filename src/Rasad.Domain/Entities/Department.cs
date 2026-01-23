namespace Rasad.Domain.Entities;

public class Department
{
    public Guid Id { get; set; }
    public Guid DirectorateId { get; set; }
    public Directorate? Directorate { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
