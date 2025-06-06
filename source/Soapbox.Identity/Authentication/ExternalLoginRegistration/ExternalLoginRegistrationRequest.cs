namespace Soapbox.Identity.Authentication.ExternalLoginRegistration;

using System.ComponentModel.DataAnnotations;

public class ExternalLoginRegistrationRequest
{
    public string ProviderDisplayName { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    // TODO: Verify
    [Required]
    [MinLength(8)]
    [Display(Name = "Username", Prompt = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
