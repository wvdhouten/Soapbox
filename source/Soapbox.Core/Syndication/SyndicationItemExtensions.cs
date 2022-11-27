namespace Soapbox.Core.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Syndication;
    using System.Xml.Linq;

    public static class SyndicationItemExtensions
    {
        public static SyndicationItem WithBaseUri(this SyndicationItem item, Uri uri)
        {
            item.BaseUri = uri;

            return item;
        }

        public static SyndicationItem WithTitle(this SyndicationItem item, string title)
        {
            item.Title = new TextSyndicationContent(title);

            return item;
        }

        public static SyndicationItem WithContent(this SyndicationItem item, string content)
        {
            item.Content = new TextSyndicationContent(content, TextSyndicationContentKind.Html);

            return item;
        }

        public static SyndicationItem WithPublishDate(this SyndicationItem item, DateTime publishDate)
        {
            item.PublishDate = publishDate;

            return item;
        }

        public static SyndicationItem WithImage(this SyndicationItem item, Uri uri)
        {
            var url = uri.ToString();
            var knownMimeType = MimeTypes.TryGetMimeType(url, out var mimeType);

            var element = new XElement("enclosure", new XAttribute("url", url), new XAttribute("length", 0), new XAttribute("type", knownMimeType ? mimeType : string.Empty));

            item.ElementExtensions.Add(element);

            return item;
        }

        public static SyndicationItem WithCategories(this SyndicationItem item, IEnumerable<string> categories)
        {
            foreach (var category in categories)
            {
                item.Categories.Add(new SyndicationCategory(category));
            }

            return item;
        }

        public static SyndicationItem WithAuthor(this SyndicationItem item, string author)
        {
            item.Authors.Add(new SyndicationPerson(null, author, null));

            return item;
        }
    }
}
