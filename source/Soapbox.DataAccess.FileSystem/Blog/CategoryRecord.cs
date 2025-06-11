namespace Soapbox.DataAccess.FileSystem.Blog;

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
}
