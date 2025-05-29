namespace Soapbox.Identity.Authentication.RecoveryCodeLogin;

using System.ComponentModel.DataAnnotations;

public class RecoveryCodeLoginRequest
{
    public string? ReturnUrl { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; } = string.Empty;
}
