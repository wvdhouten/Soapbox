namespace Soapbox.Web.Behavior.Pages;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Alkaline64.Injectable;

[Injectable]
public class GetPageViewHandler
{
    private const string ContentViewPath = "~/Content/Pages/{0}.cshtml";

    private readonly ICompositeViewEngine _viewEngine;

    public GetPageViewHandler(ICompositeViewEngine viewEngine)
    {
        _viewEngine = viewEngine;
    }

    public string? GetPageView(string page, HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(page))
            return null;

        var viewPath = string.Format(ContentViewPath, page);
        var viewExists = _viewEngine.GetView(null, viewPath, true).Success
            || _viewEngine.FindView(new ActionContext(httpContext, new RouteData(), new()), viewPath, true).Success;

        return viewExists ? viewPath : null;
    }
}
