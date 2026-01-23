using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models;

public class MinistryFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "MinistryNameLabel")]
    public string NameAr { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "MinistryCodeLabel")]
    public string Code { get; set; } = string.Empty;

    [Display(ResourceType = typeof(SharedResources), Name = "IsActiveLabel")]
    public bool IsActive { get; set; } = true;
}
