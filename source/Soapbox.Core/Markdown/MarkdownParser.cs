namespace Soapbox.Core.Markdown
{
    using System.Linq;
    using Markdig;
    using Markdig.Syntax;
    using Markdig.Syntax.Inlines;

    public class MarkdownParser : IMarkdownParser
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownParser()
        {
            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public string ToHtml(string content, out string image)
        {
            var parsed = Markdown.Parse(content, _pipeline);

            image = parsed.Descendants<ParagraphBlock>().SelectMany(x => x.Inline.Descendants<LinkInline>()).FirstOrDefault(l => l.IsImage)?.Url ?? string.Empty;

            return parsed.ToHtml();
        }
    }
}
