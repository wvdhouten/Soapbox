namespace Soapbox.Identity.Authentication.ForgotPassword;

using System.ComponentModel.DataAnnotations;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email", Prompt = "Email")]
    public required string Email { get; set; }
}
