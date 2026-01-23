using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models.Users;

public class MinistryUserCreateViewModel
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "UserNameLabel")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "EmailLabel")]
    [EmailAddress(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "InvalidEmail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "PasswordLabel")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(ResourceType = typeof(SharedResources), Name = "MinistryLabel")]
    public Guid? MinistryId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "DirectorateLabel")]
    public Guid? DirectorateId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "DepartmentLabel")]
    public Guid? DepartmentId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "SectionLabel")]
    public Guid? SectionId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "UnitLabel")]
    public Guid? UnitId { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "CanImportExcelLabel")]
    public bool CanImportExcel { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "CanExportExcelLabel")]
    public bool CanExportExcel { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "CanViewNationalIdFullLabel")]
    public bool CanViewNationalIdFull { get; set; }
}
