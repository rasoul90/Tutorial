namespace Rasad.Web.Models.Reports;

public class DuplicateEmployeeGroupViewModel
{
    public string NationalIdDisplay { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public List<string> Ministries { get; set; } = new();
}
