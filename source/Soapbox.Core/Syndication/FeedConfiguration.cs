namespace Soapbox.Application.Syndication;

using System;

public class FeedConfiguration<T>
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required Uri Link { get; set; }
}
