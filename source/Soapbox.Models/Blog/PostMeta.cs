namespace Soapbox.Domain.Blog;

public class PostMetadata
{
    public long Id { get; set; }

    public required string Type { get; set; }

    public required string Value { get; set; }
}
