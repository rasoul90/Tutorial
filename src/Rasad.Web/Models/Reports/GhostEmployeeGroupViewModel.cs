namespace Rasad.Web.Models.Reports;

public class GhostEmployeeGroupViewModel
{
    public string NationalIdDisplay { get; set; } = string.Empty;
    public int NamesCount { get; set; }
    public List<string> Names { get; set; } = new();
}
