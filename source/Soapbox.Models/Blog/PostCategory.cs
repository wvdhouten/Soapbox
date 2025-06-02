namespace Soapbox.Domain.Blog;

using System.Collections.Generic;

public record PostCategory
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public IList<Post> Posts { get; set; } = [];
}