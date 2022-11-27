namespace Soapbox.Web.Models.Account
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;

    public class LoginExternalModel
    {
        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [Required]
        [MinLength(8)]
        [Display(Name = "Username", Prompt = "Username")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
