namespace Soapbox.Web.Controllers
{
    using System;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.AspNetCore.Mvc;
    using Soapbox.Core.Markdown;
    using Soapbox.Core.Syndication;
    using Soapbox.Domain.Blog;

    public class FeedController : Controller
    {
        private enum FeedFormat
        {
            Rss,
            Atom
        }

        private readonly IBlogService _blogService;
        private readonly IMarkdownParser _markdownParser;

        public FeedController(IBlogService blogService, IMarkdownParser markdownParser)
        {
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

            var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/blog/");
            var builder = new FeedBuilder("Soapbox", "Get on your soapbox!", baseUri);
            var feed = builder.GetFeed();

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
