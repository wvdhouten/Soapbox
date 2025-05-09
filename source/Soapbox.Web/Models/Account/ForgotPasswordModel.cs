namespace Soapbox.Web.Models.Account;

using System.ComponentModel.DataAnnotations;

public class ForgotPasswordModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email", Prompt = "Email")]
    public string Email { get; set; }
}
