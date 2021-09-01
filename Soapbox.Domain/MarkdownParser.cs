namespace Soapbox.Domain
{
    using Markdig;
    using Soapbox.Domain.Abstractions;

    public class MarkdownParser : IMarkdownParser
    {
        public string Parse(string content)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            return Markdown.ToHtml(content, pipeline);
        }
    }
}
