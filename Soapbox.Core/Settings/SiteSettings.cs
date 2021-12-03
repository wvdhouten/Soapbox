namespace Soapbox.Core.Settings
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
        /// <value>
        /// The site title.
        /// </value>
        public string Title { get; set; } = "Soapbox";

        /// <summary>
        /// Gets or sets the site subtitle.
        /// </summary>
        /// <value>
        /// The site subtitle.
        /// </value>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the site description.
        /// </summary>
        /// <value>
        /// The site description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the site keywords.
        /// </summary>
        /// <value>
        /// The site keywords.
        /// </value>
        public string Keywords { get; set; }

        /// <summary>
        /// Gets or sets the site theme.
        /// </summary>
        /// <value>
        /// The site theme.
        /// </value>
        public string Theme { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        [Display(Name = "Owner")]
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the owner email address.
        /// </summary>
        /// <value>
        /// The owner email address.
        /// </value>
        [Display(Name = "Owner email")]
        public string OwnerEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to [allow registration].
        /// </summary>
        /// <value>
        ///   <c>true</c> if to [allow registration]; otherwise, <c>false</c>.
        /// </value>
        [Display(Name = "Allow user registration")]
        public bool AllowRegistration { get; set; } = false;

        /// <summary>
        /// Gets or sets the blog page layout.
        /// </summary>
        /// <value>
        /// The blog page layout.
        /// </value>
        [Display(Name = "Blog page layout")]
        public BlogPageLayout BlogPageLayout { get; set; } = BlogPageLayout.PostList;

        /// <summary>
        /// Gets or sets the posts per page.
        /// </summary>
        /// <value>
        /// The posts per page.
        /// </value>
        [Display(Name = "Posts per page")]
        public int PostsPerPage { get; set; } = 6;

        /// <summary>
        /// Gets or sets the posts per page.
        /// </summary>
        /// <value>
        /// The posts per page.
        /// </value>
        public string SyndicationFeedImage { get; set; }
    }
}
