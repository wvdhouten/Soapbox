namespace Soapbox.Core.Syndication
{
    using System;

    public class FeedConfiguration<T>
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public Uri Link { get; set; }
    }
}
