namespace Soapbox.Core.Markdown
{
    using Markdig;

    public class MarkdownParser : IMarkdownParser
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownParser()
        {
            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public string Parse(string content)
        {
            return Markdown.ToHtml(content, _pipeline);
        }
    }
}
