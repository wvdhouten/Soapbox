namespace Soapbox.Web.Controllers
{
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.AspNetCore.Mvc;

    public class RobotsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("/rsd.xml")]
        public async Task RsdXml()
        {
            var host = $"{Request.Scheme}://{Request.Host}";

            Response.ContentType = "application/xml";
            Response.Headers["cache-control"] = "no-cache, no-store, must-revalidate";

            var settings = new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };

            // TODO: Nicer way of writing this?
            await using var writer = XmlWriter.Create(Response.Body, settings);
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
}
