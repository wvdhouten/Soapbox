namespace Soapbox.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Behavior.Pages;
using Soapbox.Web.Controllers.Base;

[Route("Pages")]
public class PagesController : SoapboxControllerBase
{
    [HttpGet("{page}")]
    public IActionResult ShowPage(
        [FromServices] GetPageViewHandler handler, 
        [FromRoute] string page)
    {
        var viewPath = handler.GetPageView(page, HttpContext);
        return viewPath switch
        {
            null => NotFound(),
            _ => View(viewPath),
        };
    }
}
