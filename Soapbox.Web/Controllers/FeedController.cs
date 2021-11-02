namespace Soapbox.Web.Controllers
{
    using System;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Markdown;
    using Soapbox.Core.Settings;
    using Soapbox.Core.Syndication;
    using Soapbox.Domain.Abstractions;

    public class FeedController : Controller
    {
        private enum FeedFormat
        {
            Rss,
            Atom
        }

        private readonly SiteSettings _settings;
        private readonly IBlogService _blogService;
        private readonly IMarkdownParser _markdownParser;

        public FeedController(IOptionsSnapshot<SiteSettings> config, IBlogService blogService, IMarkdownParser markdownParser)
        {
            _settings = config.Value;
            _blogService = blogService;
            _markdownParser = markdownParser;
        }

        public async Task Rss()
        {
            await OutputFeed(FeedFormat.Rss);
        }

        public async Task Atom()
        {
            await OutputFeed(FeedFormat.Atom);
        }

        private async Task OutputFeed(FeedFormat format = FeedFormat.Rss)
        {
            Response.ContentType = "text/xml";

            var siteUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            var builder = new FeedBuilder(new Uri(siteUri, "/blog/"), _settings.Title, _settings.Description, new Uri(siteUri, "/assets/yaktocat.png"));
            builder.AddCopyright(_settings.Owner);
            var feed = builder.GetFeed(_settings.AdminEmail);

            var posts = await _blogService.GetPostsAsync(15, 0);

            await feed.WithItemsAsync(posts, post =>
            {
                var item = new SyndicationItem()
                    .WithTitle(post.Title)
                    .WithContent(_markdownParser.Parse(post.Content));

                return item;
            });

            var settings = new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };

            await using var writer = XmlWriter.Create(Response.Body, settings);

            SyndicationFeedFormatter formatter = format switch
            {
                FeedFormat.Atom => feed.GetAtom10Formatter(),
                _ => feed.GetRss20Formatter(false),
            };

            formatter.WriteTo(writer);
            await writer.FlushAsync();
        }
    }
}
