namespace HRGov.HRApp.ViewModels;

public class LookupCollections
{
    public IReadOnlyCollection<LookupItem> Departments { get; set; } = Array.Empty<LookupItem>();
    public IReadOnlyCollection<LookupItem> JobTitles { get; set; } = Array.Empty<LookupItem>();
}

public record LookupItem(int Id, string Name);
