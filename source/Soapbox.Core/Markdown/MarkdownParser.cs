namespace Soapbox.Application.Markdown;

using System.Linq;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Soapbox.Domain.Markdown;

public class MarkdownParser : IMarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownParser()
    {
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    public string ToHtml(string content, out IEnumerable<string> images)
    {
        var parsed = Markdown.Parse(content, _pipeline);

        images = parsed.Descendants<ParagraphBlock>()
            .SelectMany(x => x.Inline.Descendants<LinkInline>())
            .Where(l => l.IsImage).Select(l => l.Url);

        return parsed.ToHtml();
    }
}
