namespace Soapbox.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Soapbox.Application.Rsd;
using Soapbox.Web.Controllers.Base;
using System.Threading.Tasks;

[Route("[controller]")]
public class RobotsController : SoapboxControllerBase
{
    [Route("/robots.txt")]
    public IActionResult Index()
    {
        // TODO: Implement robots.txt generation logic
        return View();
    }

    [Route("/rsd.xml")]
    [Produces("application/xml")]
    public async Task RsdXml([FromServices] RsdGenerator generator)
    {
        Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";

        await generator.GenerateRsdAsync(Response.Body, Request);
    }
}
