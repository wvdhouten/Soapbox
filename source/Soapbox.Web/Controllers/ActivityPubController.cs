namespace Soapbox.Web.Controllers;
using System;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Controllers.Base;

[Route(".well-known")]
public class ActivityPubController : SoapboxControllerBase
{
    [Produces("application/jrd+json")]
    [HttpGet("webfinger")]
    public ActionResult WebFinger([FromQuery] string resource) 
        => throw new NotImplementedException("WebFinger is not implemented yet.");
}
