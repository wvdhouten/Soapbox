namespace Soapbox.Core.Syndication
{
    using System;
    using System.ServiceModel.Syndication;

    public class FeedConfiguration<T>
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public Uri Link { get; set; }
    }
}
