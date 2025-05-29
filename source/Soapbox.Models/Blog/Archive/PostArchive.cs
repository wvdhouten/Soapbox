namespace Soapbox.Domain.Blog.Archive;

using System;
using System.Collections.Generic;

public class PostArchive
{
    public DayOfWeek StartOfWeek { get; set; } = DayOfWeek.Monday;

    public int FirstYear { get; set; } = DateTime.UtcNow.Year;

    public int Year { get; set; } = DateTime.UtcNow.Year;

    public int? Month { get; set; }

    public IDictionary<DateTime, ICollection<Post>> Days { get; } = new Dictionary<DateTime, ICollection<Post>>();
}
