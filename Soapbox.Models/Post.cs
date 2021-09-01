namespace Soapbox.Models
{
    using System;
    using System.Collections.Generic;

    public class Post
    {
        public string Id { get; set; }

        public string Author { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Excerpt { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedOn { get; set; } = null;

        public IList<string> Categories { get; } = new List<string>();
    }
}
