namespace Soapbox.Web.Helpers;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Mvc.Razor;
using Soapbox.Application.Utils;

[Injectable<IViewEngineManager>]
public class ViewEngineManager : IViewEngineManager
{
    private readonly IRazorViewEngine _viewEngine;

    public ViewEngineManager(IRazorViewEngine viewEngine)
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
    }

    public void ClearCache() => _viewEngine.TryInvokeMethod(typeof(RazorViewEngine), "ClearCache");
}
