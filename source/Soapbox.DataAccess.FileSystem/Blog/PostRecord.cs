namespace Soapbox.DataAccess.FileSystem.Blog;

using System;
using System.Collections.Generic;
using Soapbox.Domain.Blog;
using YamlDotNet.Serialization;

/// <summary>
/// Represents a blog post.
/// </summary>
internal record PostRecord
{
    /// <summary>
    /// Gets or sets the post identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the post title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author identifier.
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;

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
    [YamlIgnore]
    public string Content { get; set; } = string.Empty;

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
    public DateTime PublishedOn { get; set; } = DateTime.MaxValue;

    /// <summary>
    /// Gets or sets the categories.
    /// </summary>
    public IList<string> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the meta data.
    /// </summary>
    public IList<PostMetadata> Metadata { get; set; } = [];

    public static implicit operator Post(PostRecord record)
        => new()
        {
            Id = record.Id,
            Title = record.Title,
            Slug = record.Slug,
            Excerpt = record.Excerpt,
            Content = record.Content,
            ModifiedOn = record.ModifiedOn,
            Status = record.Status,
            PublishedOn = record.PublishedOn,
            Categories = [.. record.Categories.Select(category => new PostCategory() { Name = category })],
            Metadata = record.Metadata,
        };


    public static implicit operator PostRecord(Post post)
        => new()
        {
            Id = post.Id!,
            Title = post.Title,
            Slug = post.Slug,
            AuthorId = post.Author.Id,
            Excerpt = post.Excerpt,
            Content = post.Content,
            ModifiedOn = post.ModifiedOn,
            Status = post.Status,
            PublishedOn = post.PublishedOn,
            Categories = [.. post.Categories.Select(category => category.Name)],
            Metadata = post.Metadata,
        };
}
