namespace Soapbox.Core.Syndication
{
    using System.ServiceModel.Syndication;

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
    }
}
