namespace Soapbox.Identity.Authenticators.AddAuthenticator;

using System.ComponentModel.DataAnnotations;

public class AddAuthenticatorRequest
{
    public string SharedKey { get; set; } = string.Empty;

    public string AuthenticatorUri { get; set; } = string.Empty;

    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Verification Code")]
    public string Code { get; set; } = string.Empty;
}
