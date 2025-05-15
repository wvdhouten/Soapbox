namespace Soapbox.Web.Models.Account;

using System.ComponentModel.DataAnnotations;

public class LoginRecoveryCodeModel
{
    public string? ReturnUrl { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; } = string.Empty;
}
