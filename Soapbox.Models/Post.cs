namespace Soapbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

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
        /// Gets or sets the post title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author identifier.
        /// </summary>
        public SoapboxUser Author { get; set; }

        /// <summary>
        /// Gets or sets the post slug.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the post excerpt.
        /// </summary>
        public string Excerpt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the post content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        [Display(Name = "Modified on"), DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the post status.
        /// </summary>
        public PostStatus Status { get; set; } = PostStatus.Private;

        /// <summary>
        /// Gets or sets the publication date.
        /// </summary>
        [Display(Name = "Published on"), DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime PublishedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        public IList<PostCategory> Categories { get; set; } = new List<PostCategory>();

        /// <summary>
        /// Gets or sets the meta data.
        /// </summary>
        public IList<PostMeta> Meta { get; set; } = new List<PostMeta>();
    }

    /// <summary>
    /// The status of a post.
    /// </summary>
    public enum PostStatus
    {
        /// <summary>
        /// Published
        /// </summary>
        Published,

        /// <summary>
        /// Private
        /// </summary>
        Private
    }
}
