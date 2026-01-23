using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models;

public class MinistryDataWindowFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "MinistryLabel")]
    public Guid? MinistryId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "StartAtLabel")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "EndAtLabel")]
    public DateTime EndAt { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "IsActiveLabel")]
    public bool IsActive { get; set; }

    [Display(ResourceType = typeof(SharedResources), Name = "AfterCloseMessageLabel")]
    public string AfterCloseMessageAr { get; set; } = string.Empty;
}
