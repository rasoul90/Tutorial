using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models;

public class UnitFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "SectionLabel")]
    public Guid? SectionId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    [Display(ResourceType = typeof(SharedResources), Name = "UnitNameLabel")]
    public string NameAr { get; set; } = string.Empty;
}
