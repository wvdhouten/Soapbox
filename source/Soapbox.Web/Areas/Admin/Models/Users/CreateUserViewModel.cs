namespace Soapbox.Web.Areas.Admin.Models.Users
{
    using System.ComponentModel.DataAnnotations;
    using Soapbox.Domain.Users;

    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [Display(Name = "Username")]
        public required string Username { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [Display(Name = "Display Name")]
        public required string DisplayName { get; set; }

        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }
    }
}
