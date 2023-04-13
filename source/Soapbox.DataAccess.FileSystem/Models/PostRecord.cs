namespace Soapbox.DataAccess.FileSystem.Models
{
    using System;
    using System.Collections.Generic;
    using Soapbox.Models;

    /// <summary>
    /// Represents a blog post.
    /// </summary>
    public record PostRecord
    {
        /// <summary>
        /// Gets or sets the post identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the post title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author identifier.
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// Gets or sets the post slug.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the post excerpt.
        /// </summary>
        public string Excerpt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the post status.
        /// </summary>
        public PostStatus Status { get; set; } = PostStatus.Draft;

        /// <summary>
        /// Gets or sets the publication date.
        /// </summary>
        public DateTime PublishedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        public IList<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the meta data.
        /// </summary>
        public IList<PostMetadata> Metadata { get; set; } = new List<PostMetadata>();
    }
}
