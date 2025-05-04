namespace Soapbox.Domain.Markdown;

public interface IMarkdownParser
{
    public string ToHtml(string content, out IEnumerable<string> images);
}
