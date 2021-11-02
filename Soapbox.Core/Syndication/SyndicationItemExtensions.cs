namespace Soapbox.Core.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Syndication;
    using System.Xml.Linq;

    public static class SyndicationItemExtensions
    {
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
            var element = new XElement("enclosure", new XAttribute("url", uri.ToString()), new XAttribute("length", 0), new XAttribute("type", "image/png"));

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

        public static SyndicationItem WithContributors(this SyndicationItem item, IEnumerable<string> contributors)
        {
            foreach (var contributor in contributors)
            {
                item.Contributors.Add(new SyndicationPerson(contributor));
            }

            return item;
        }
    }
}
