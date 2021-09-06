namespace Soapbox.Web.Config
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the configurable site settings.
    /// </summary>
    public class SiteSettings
    {
        /// <summary>
        /// Gets or sets the site title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the site description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the administrator email.
        /// </summary>
        public string AdminEmail { get; set; }

        /// <summary>
        /// Gets or sets whether user registration is allowed.
        /// </summary>
        [Display(Name="Allow user registration")]
        public bool AllowRegistration { get; set; } = false;
    }
}
