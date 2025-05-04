namespace Soapbox.Application.Syndication;

using System;
using System.ServiceModel.Syndication;

public static class SyndicationFeedExtensions
{
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

    public static SyndicationFeedFormatter GetFormatter(this SyndicationFeed feed, FeedFormat format)
    {
        return format switch
        {
            FeedFormat.Atom => feed.GetAtom10Formatter(),
            FeedFormat.Rss => feed.GetRss20Formatter(false),
            _ => throw new NotImplementedException(),
        };
    }
}
