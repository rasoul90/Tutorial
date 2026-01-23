using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "UserNameRequired")]
    [Display(ResourceType = typeof(SharedResources), Name = "UserNameLabel")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "PasswordRequired")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(SharedResources), Name = "PasswordLabel")]
    public string Password { get; set; } = string.Empty;
}
