namespace Soapbox.Domain.Blog;

using System;

public class Media
{
    public required string Name { get; set; }

    public long Size { get; set; }

    public DateTime ModifiedOn { get; set; }
}
