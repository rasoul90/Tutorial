using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models.Employees;

public class EmployeeFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "EmployeeFullNameLabel")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "JobNumberLabel")]
    public string JobNumber { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "NationalIdLabel")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
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
}
