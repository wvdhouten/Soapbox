namespace Soapbox.Core.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Syndication;
    using System.Threading.Tasks;

    public static class SyndicationFeedExtensions
    {
        public static SyndicationFeed WithImage(this SyndicationFeed feed, Uri uri)
        {
            feed.ImageUrl = uri;
            return feed;
        }

        public static SyndicationFeed WithCopyright(this SyndicationFeed feed, string owner)
        {
            feed.Copyright = new TextSyndicationContent($"Copyright {DateTime.UtcNow.Year}, {owner}");
            return feed;
        }

        public static SyndicationFeed WithCategory(this SyndicationFeed feed, string name)
        {
            feed.Categories.Add(new SyndicationCategory(name));
            return feed;
        }

        public static SyndicationFeed WithManagingEditor(this SyndicationFeed feed, string managingEditor)
        {
            feed.ElementExtensions.Add("managingEditor", string.Empty, managingEditor);
            return feed;
        }

        public static SyndicationFeed WithWebMaster(this SyndicationFeed feed, string webMaster)
        {
            feed.ElementExtensions.Add("webMaster", string.Empty, webMaster);
            return feed;
        }

        public static SyndicationFeed WithGenerator(this SyndicationFeed feed, string name, string version, Uri uri)
        {
            feed.Generator = $"{name} {version} ({uri})";
            return feed;
        }

        public static SyndicationFeed WithItems<T>(this SyndicationFeed feed, IEnumerable<T> items, Func<T, SyndicationItem> itemMapper)
        {
            var syndicationItems = items.Select(itemMapper);

            if (syndicationItems.Any())
            {
                feed.LastUpdatedTime = syndicationItems.FirstOrDefault().LastUpdatedTime;
            }

            feed.Items = syndicationItems;

            return feed;
        }

        public async static Task<SyndicationFeed> WithItemsAsync<T>(this SyndicationFeed feed, IAsyncEnumerable<T> items, Func<T, SyndicationItem> itemMapper)
        {
            var syndicationItems = new List<SyndicationItem>();
            await foreach (var item in items)
            {
                syndicationItems.Add(itemMapper(item));
            }

            if (syndicationItems.Any())
            {
                feed.LastUpdatedTime = syndicationItems.FirstOrDefault().LastUpdatedTime;
            }

            feed.Items = syndicationItems;

            return feed;
        }
    }
}
