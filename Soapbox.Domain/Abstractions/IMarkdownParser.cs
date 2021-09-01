namespace Soapbox.Domain.Abstractions
{
    public interface IMarkdownParser
    {
        public string Parse(string content);
    }
}
