namespace Soapbox.Web.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;

[Route(".well-known")]
public class ActivityPubController : Controller
{
    [Produces("application/jrd+json")]
    [HttpGet("webfinger")]
    public ActionResult WebFinger([FromQuery] string resource)
    {
        throw new NotImplementedException("WebFinger is not implemented yet.");
    }
}
