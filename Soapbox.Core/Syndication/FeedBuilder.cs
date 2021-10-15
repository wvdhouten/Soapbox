namespace Soapbox.Core.Syndication
{
    using System;
    using System.ServiceModel.Syndication;

    public class FeedBuilder
    {
        private readonly Uri _baseUri;
        private readonly SyndicationFeed _feed;

        public FeedBuilder(string title, string description, Uri baseUri)
        {
            _baseUri = baseUri;

            _feed = new SyndicationFeed(title, description, baseUri)
            {
                Id = baseUri.ToString(),
            };
        }

        public SyndicationFeed GetFeed()
        {
            _feed.WithImage(new Uri("https://localhost/TODO.png"))
                 .WithCopyright("TODO")
                 .WithManagingEditor("TODO")
                 .WithWebMaster("TODO")
                 .WithGenerator("Soapbox", GetType().Assembly.GetName().Version.ToString(), new Uri("https://github.com/wvdhouten/Soapbox"));

            return _feed;
        }
    }
}
