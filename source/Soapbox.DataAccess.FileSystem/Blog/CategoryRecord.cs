namespace Soapbox.DataAccess.FileSystem.Blog;

using Soapbox.Domain.Blog;

/// <summary>
/// Represents a category post.
/// </summary>
internal record CategoryRecord
{
    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    public static implicit operator PostCategory(CategoryRecord record)
    => new()
    {
        Id = record.Id,
        Slug = record.Slug,
        Name = record.Name,
    };


    public static implicit operator CategoryRecord(PostCategory category)
        => new()
        {
            Id = category.Id,
            Slug = category.Slug,
            Name = category.Name,
        };
}
