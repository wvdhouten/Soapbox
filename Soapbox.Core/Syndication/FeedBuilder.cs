namespace Soapbox.Core.Syndication
{
    using System;
    using System.ServiceModel.Syndication;

    public class FeedBuilder
    {
        private readonly Uri _baseUri;
        private readonly SyndicationFeed _feed;

        public FeedBuilder(Uri baseUri, string title, string description, Uri imageUri)
        {
            _baseUri = baseUri;

            _feed = new SyndicationFeed(title, description, baseUri)
            {
                Id = baseUri.ToString(),
            }
            .WithImage(imageUri)
            .WithGenerator("Soapbox", GetType().Assembly.GetName().Version.ToString(), new Uri("https://github.com/wvdhouten/Soapbox"));
        }

        public void AddCopyright(string owner)
        {
            _feed.WithCopyright(owner);
        }

        // TODO
        public SyndicationFeed GetFeed(string administrator)
        {
            _feed.WithManagingEditor(administrator)
                 .WithWebMaster(administrator);

            return _feed;
        }
    }
}
