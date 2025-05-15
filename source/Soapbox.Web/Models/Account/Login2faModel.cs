namespace Soapbox.Web.Models.Account;

using System.ComponentModel.DataAnnotations;

public class Login2faModel
{
    public string? ReturnUrl { get; set; }

    public bool RememberMe { get; set; }

    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Authenticator code")]
    public string AuthenticatorCode { get; set; } = string.Empty;

    [Display(Name = "Remember this machine")]
    public bool RememberMachine { get; set; }
}
