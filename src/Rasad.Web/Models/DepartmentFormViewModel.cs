using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models;

public class DepartmentFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "DirectorateLabel")]
    public Guid? DirectorateId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "DepartmentNameLabel")]
    public string NameAr { get; set; } = string.Empty;
}
