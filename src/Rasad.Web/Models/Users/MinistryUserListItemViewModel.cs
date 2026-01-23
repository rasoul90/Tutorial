namespace Rasad.Web.Models.Users;

public class MinistryUserListItemViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MinistryName { get; set; } = string.Empty;
    public string DirectorateName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? SectionName { get; set; }
    public string? UnitName { get; set; }
    public bool CanImportExcel { get; set; }
    public bool CanExportExcel { get; set; }
    public bool CanViewNationalIdFull { get; set; }
}
