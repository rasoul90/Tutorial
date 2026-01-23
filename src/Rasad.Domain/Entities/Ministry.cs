namespace Rasad.Domain.Entities;

public class Ministry
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Directorate> Directorates { get; set; } = new List<Directorate>();
    public ICollection<MinistryDataWindow> DataWindows { get; set; } = new List<MinistryDataWindow>();
}
