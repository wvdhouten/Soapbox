namespace Soapbox.Web.Controllers;

using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Soapbox.Application.Extensions;
using Soapbox.Application.Settings;
using Soapbox.Application.Syndication;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Markdown;

public class FeedController : Controller
{
    private readonly SiteSettings _settings;
    private readonly IBlogRepository _blogService;
    private readonly IMarkdownParser _markdownParser;

    public FeedController(IOptionsSnapshot<SiteSettings> settings, IBlogRepository blogService, IMarkdownParser markdownParser)
    {
        _settings = settings.Value;
        _blogService = blogService;
        _markdownParser = markdownParser;
    }

    [Produces("application/rss+xml")]
    public async Task Rss()
    {
        await WriteFeedAsync(FeedFormat.Rss);
    }

    [Produces("application/atom+xml")]
    public async Task Atom()
    {
        await WriteFeedAsync(FeedFormat.Atom);
    }

    private async Task WriteFeedAsync(FeedFormat format = FeedFormat.Rss)
    {
        var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
        var imageUri = !string.IsNullOrWhiteSpace(_settings.SyndicationFeedImage)
            ? _settings.SyndicationFeedImage.StartsWith("http")
                ? new Uri(_settings.SyndicationFeedImage)
                : new Uri(baseUri, _settings.SyndicationFeedImage)
            : null;
        var blogUri = new Uri(baseUri, "blog/");

        var builder = new FeedBuilder(new Uri(baseUri, "blog/"), _settings.Title, _settings.Description);
        builder.SetSelfLink(new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}"));
        builder.SetImage(imageUri);
        builder.SetOwner(_settings.Owner, _settings.OwnerEmail);
        builder.SetCategories(_settings.Keywords.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        var postsPage = await _blogService.GetPostsPageAsync(0, 30);
        builder.SetItems(postsPage.Items, post =>
        {
            var content = _markdownParser.ToHtml(post.Content, out var images);
            var image = images.FirstOrDefault() ?? string.Empty;
            var postUri = new Uri(blogUri, post.Slug);
            var idUri = new Uri(blogUri, $"post/{post.Id}");
            var item = new SyndicationItem(post.Title, string.Empty, postUri, idUri.AbsoluteUri, post.ModifiedOn)
            .WithBaseUri(baseUri)
            .WithContent(content)
            .WithAuthor(post.Author.ShownName)
            .WithPublishDate(post.PublishedOn);

            var imageUri = !string.IsNullOrWhiteSpace(image)
                ? image.StartsWith("http")
                    ? new Uri(image)
                    : new Uri(baseUri, image)
                : null;

            if (imageUri != null)
            {
                item.WithImage(imageUri);
            }
            item.WithCategories(post.Categories.Select(c => c.Name));

            // TODO: Add comments

            return item;
        });

        var feed = builder.GetFeed();

        var settings = new XmlWriterSettings
        {
            Async = true,
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };
        await using var writer = XmlWriter.Create(Response.Body, settings);
        var formatter = feed.GetFormatter(format);
        formatter.WriteTo(writer);
        await writer.FlushAsync();
    }
}
