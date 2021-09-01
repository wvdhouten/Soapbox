namespace Soapbox.Web.Config
{
    using System.ComponentModel.DataAnnotations;

    public class SiteSettings
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string AdminEmail { get; set; }

        [Display(Name="Allow user registration")]
        public bool AllowRegistration { get; set; } = false;
    }
}
