namespace Soapbox.Core.Markdown
{
    public interface IMarkdownParser
    {
        public string ToHtml(string content, out string image);
    }
}
