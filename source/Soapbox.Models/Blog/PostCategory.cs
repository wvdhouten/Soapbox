namespace Soapbox.Domain.Blog;

using System.Collections.Generic;

public class PostCategory
{
    public long Id { get; set; }

    public required string Name { get; set; }

    public string Slug { get; set; } = string.Empty;

    public IList<Post> Posts { get; set; } = [];
}