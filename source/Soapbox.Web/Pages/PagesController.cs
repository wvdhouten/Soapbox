namespace Soapbox.Web.Pages;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Pages.GetPage;

[Route("Pages")]
public class PagesController : SoapboxControllerBase
{
    [HttpGet("{page}")]
    public IActionResult ShowPage([FromServices] GetPageQuery query, [FromRoute] string page)
    {
        var viewPath = query.Handle(page, HttpContext);

        if (viewPath is null)
            return NotFound();

        return View(viewPath);
    }
}
