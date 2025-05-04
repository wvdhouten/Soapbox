namespace Soapbox.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Soapbox.Web.Controllers.Base;

[Route("[controller]")]
public class PagesController : SoapboxControllerBase
{
    private const string ContentViewPath = "~/Content/Pages/{0}.cshtml";

    [HttpGet("{view}")]
    public IActionResult Index(string view = "")
    {
        var viewPath = string.Format(ContentViewPath, view);
        if (!ViewExists(viewPath))
            return NotFound();

        return View(viewPath);
    }

    private bool ViewExists(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        var viewEngine = HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
        return viewEngine.GetView(null, name, true).Success
            || viewEngine.FindView(ControllerContext, name, true).Success;
    }
}
