namespace Soapbox.Web.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    public class ProfileModel
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Display name")]
            public string DisplayName { get; set; }

            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        public bool HasAuthenticator { get; set; }
    }
}
