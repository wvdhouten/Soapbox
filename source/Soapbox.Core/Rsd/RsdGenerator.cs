namespace Soapbox.Application.Rsd;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;

[Injectable]
public class RsdGenerator
{
    public async Task GenerateRsdAsync(Stream outputStream, HttpRequest request)
    {
        var host = $"{request.Scheme}://{request.Host}";

        var settings = new XmlWriterSettings
        {
            Async = true,
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };

        await using var writer = XmlWriter.Create(outputStream, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("rsd");
        writer.WriteAttributeString("version", "1.0");

        writer.WriteStartElement("service");

        writer.WriteElementString("engineName", "Soapbox");
        writer.WriteElementString("engineLink", "https://github.com/wvdhouten/Soapbox");
        writer.WriteElementString("homePageLink", host);

        writer.WriteStartElement("apis");
        writer.WriteStartElement("api");
        writer.WriteAttributeString("name", "MetaWeblog");
        writer.WriteAttributeString("preferred", "true");
        writer.WriteAttributeString("apiLink", $"{host}/livewriter");
        writer.WriteAttributeString("blogId", "1");

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();

        await writer.FlushAsync();
    }
}
