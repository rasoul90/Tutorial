using System.ComponentModel.DataAnnotations;

namespace DeliverySaaS.Web.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "اسم المستخدم")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;
}
