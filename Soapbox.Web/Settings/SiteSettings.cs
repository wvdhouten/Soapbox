namespace Soapbox.Web.Settings
{
    using System.ComponentModel.DataAnnotations;

    // TODO: Move config file to a different folder? (/content? /data?)
    /// <summary>
    /// Represents the configurable site settings.
    /// </summary>
    public class SiteSettings
    {
        /// <summary>
        /// Gets or sets the site title.
        /// </summary>
        /// <value>
        /// The site title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the site description.
        /// </summary>
        /// <value>
        /// The site description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the administrator email address.
        /// </summary>
        /// <value>
        /// The administrator email address.
        /// </value>
        public string AdminEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to [allow registration].
        /// </summary>
        /// <value>
        ///   <c>true</c> if to [allow registration]; otherwise, <c>false</c>.
        /// </value>
        [Display(Name="Allow user registration")]
        public bool AllowRegistration { get; set; } = false;
    }
}
