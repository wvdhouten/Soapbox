namespace Soapbox.Identity.Email.SendEmailVerification;

using System.ComponentModel.DataAnnotations;

public class SendEmailVerificationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
