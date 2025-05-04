namespace Soapbox.Web.Models.Account;

using System.ComponentModel.DataAnnotations;

public class SendEmailVerificationModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
