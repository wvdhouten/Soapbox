namespace Soapbox.Application.Syndication;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;

public class FeedBuilder
{
    private const string GeneratorName = "Soapbox";
    private const string GeneratorUrl = "https://github.com/wvdhouten/Soapbox";

    private readonly SyndicationFeed _feed;

    public FeedBuilder(Uri sourceUri, string title, string? description)
    {
        _feed = new SyndicationFeed(title, description, sourceUri)
        {
            Id = sourceUri.ToString(),
            LastUpdatedTime = DateTimeOffset.MinValue,
            TimeToLive = TimeSpan.FromHours(24)
        }
        .WithGenerator(GeneratorName, Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty, new Uri(GeneratorUrl));
    }

    public void SetSelfLink(Uri feedUri)
    {
        _feed.Links.Add(new SyndicationLink(feedUri, "self", null, null, 0));
    }

    public void SetOwner(string? owner, string? ownerEmail)
    {
        _feed.WithCopyright(owner)
            .WithManagingEditor(ownerEmail)
            .WithWebMaster(ownerEmail);
    }

    public void SetImage(Uri? imageUri)
    {
        _feed.ImageUrl = imageUri;
    }

    public void SetCategories(IEnumerable<string?> categories)
    {
        foreach (var category in categories)
            _feed.WithCategory(category);
    }

    public void SetItems<T>(IEnumerable<T> items, Func<T, SyndicationItem> mapper)
    {
        var syndicationItems = items.Select(mapper);
        if (syndicationItems.Any())
        {
            syndicationItems = syndicationItems.OrderByDescending(i => i.PublishDate);
            _feed.LastUpdatedTime = syndicationItems.First().LastUpdatedTime;
        }

        _feed.Items = syndicationItems;
    }

    public SyndicationFeed GetFeed()
    {
        return _feed;
    }
}
