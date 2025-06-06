namespace Soapbox.Identity.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

internal static class HelperExtensions
{
    internal static string Action(this IUrlHelper urlHelper, string controller, string action, object? values = null)
    {
        return urlHelper.Action(new UrlActionContext
        {
            Host = urlHelper.ActionContext.HttpContext.Request.Host.ToString(),
            Protocol = urlHelper.ActionContext.HttpContext.Request.Scheme,
            Controller = controller,
            Action = action,
            Values = values
        });
    }
}
