using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models;

public class DirectorateFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "MinistryLabel")]
    public Guid? MinistryId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "DirectorateNameLabel")]
    public string NameAr { get; set; } = string.Empty;
}
