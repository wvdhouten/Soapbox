namespace Soapbox.Application.Syndication;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Soapbox.Application.Settings;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Markdown;

[Injectable]
public class GenerateFeedHandler
{
    private readonly SiteSettings _settings;
    private readonly IBlogRepository _blogService;
    private readonly IMarkdownParser _markdownParser;

    public GenerateFeedHandler(IOptionsSnapshot<SiteSettings> settings, IBlogRepository blogService, IMarkdownParser markdownParser)
    {
        _settings = settings.Value;
        _blogService = blogService;
        _markdownParser = markdownParser;
    }

    public async Task GenerateFeedAsync(Stream outputStream, HttpRequest request, FeedFormat format = FeedFormat.Rss)
    {
        var baseUri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}");
        var imageUri = !string.IsNullOrWhiteSpace(_settings.SyndicationFeedImage)
            ? _settings.SyndicationFeedImage.StartsWith("http")
                ? new Uri(_settings.SyndicationFeedImage)
                : new Uri(baseUri, _settings.SyndicationFeedImage)
            : null;
        var blogUri = new Uri(baseUri, "blog/");

        var builder = new FeedBuilder(new Uri(baseUri, "blog/"), _settings.Title, _settings.Description);
        builder.SetSelfLink(new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}"));
        builder.SetImage(imageUri);
        builder.SetOwner(_settings.Owner, _settings.OwnerEmail);
        builder.SetCategories(_settings.Keywords?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? []);

        var postsPage = await _blogService.GetPostsPageAsync();
        builder.SetItems(postsPage.Items, post =>
        {
            return GetPostSyndicationItem(post, baseUri, blogUri);
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

        await using var writer = XmlWriter.Create(outputStream, settings);
        var formatter = feed.GetFormatter(format);
        formatter.WriteTo(writer);
        await writer.FlushAsync();
    }

    private SyndicationItem GetPostSyndicationItem(Post post, Uri baseUri, Uri blogUri)
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
            item.WithImage(imageUri);
        item.WithCategories(post.Categories.Select(c => c.Name));

        // TODO: Add comments

        return item;
    }
}
