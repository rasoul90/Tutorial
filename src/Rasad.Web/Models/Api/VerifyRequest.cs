using System.ComponentModel.DataAnnotations;
using Rasad.Web.Resources;

namespace Rasad.Web.Models.Api;

public class VerifyRequest
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    public string ClientId { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    public string Timestamp { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    public string Nonce { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    public string Signature { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    public Guid MinistryId { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredField")]
    public string NationalId { get; set; } = string.Empty;
}
