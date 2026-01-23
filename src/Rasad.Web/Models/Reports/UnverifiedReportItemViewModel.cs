namespace Rasad.Web.Models.Reports;

public class UnverifiedReportItemViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string JobNumber { get; set; } = string.Empty;
    public string NationalIdDisplay { get; set; } = string.Empty;
    public string MinistryName { get; set; } = string.Empty;
    public string DirectorateName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? SectionName { get; set; }
    public string? UnitName { get; set; }
}
