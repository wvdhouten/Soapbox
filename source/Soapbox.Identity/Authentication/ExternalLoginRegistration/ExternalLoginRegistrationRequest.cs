namespace Soapbox.Identity.Authentication.ExternalLoginRegistration;

using System.ComponentModel.DataAnnotations;

public class ExternalLoginRegistrationRequest
{
    public string ProviderDisplayName { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email", Prompt = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [Display(Name = "Username", Prompt = "Username")]
    public string Username { get; set; } = string.Empty;
}
