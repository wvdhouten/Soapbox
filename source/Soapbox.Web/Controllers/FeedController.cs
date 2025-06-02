namespace Soapbox.Web.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Syndication;
using Soapbox.Web.Controllers.Base;

public class FeedController : SoapboxControllerBase
{
    [Produces("application/rss+xml")]
    public async Task Rss([FromServices] GenerateFeedHandler handler)
        => await handler.GenerateFeedAsync(Response.Body, Request, FeedFormat.Rss);

    [Produces("application/atom+xml")]
    public async Task Atom([FromServices] GenerateFeedHandler handler)
        => await handler.GenerateFeedAsync(Response.Body, Request, FeedFormat.Atom);
}
