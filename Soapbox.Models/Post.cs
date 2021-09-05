namespace Soapbox.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a blog post.
    /// </summary>
    public class Post
    {
        /// <summary>
        /// Gets or sets the post identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the author identifier.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the post title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the post content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the post excerpt. *optional
        /// </summary>
        public string Excerpt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the post slug.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the publication date.
        /// </summary>
        public DateTime? PublishedOn { get; set; } = null;

        // TODO: Re-evaluate how to manage categories.
        /// <summary>
        /// Gets or sets the post categories.
        /// </summary>
        public IList<string> Categories { get; } = new List<string>();
    }
}
