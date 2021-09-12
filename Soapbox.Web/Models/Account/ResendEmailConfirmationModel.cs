namespace Soapbox.Web.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    public class ResendEmailConfirmationModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
