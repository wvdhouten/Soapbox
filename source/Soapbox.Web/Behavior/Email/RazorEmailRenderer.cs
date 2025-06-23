namespace Soapbox.Web.Behavior.Email;
using System;
using System.IO;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Email;

[Injectable<IEmailRenderer>]
public class RazorEmailRenderer : IEmailRenderer
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICompositeViewEngine _viewEngine;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly ILogger<RazorEmailRenderer> _logger;

    public RazorEmailRenderer(
        IHttpContextAccessor httpContextAccessor,
        ICompositeViewEngine viewEngine,
        IServiceProvider serviceProvider,
        ITempDataProvider tempDataProvider,
        ILogger<RazorEmailRenderer> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _viewEngine = viewEngine;
        _serviceProvider = serviceProvider;
        _tempDataProvider = tempDataProvider;
        _logger = logger;
    }

    public async Task<string> Render<TModel>(string templateName, TModel model)
    {
        var actionContext = GetActionContext();

        // TODO: Maybe also search the Content folder
        // Note: You can also support multiple languages by separating each locale into a folder
        var result = _viewEngine.GetView(null, templateName, true);
        if (result.Success == false)
            result = _viewEngine.FindView(actionContext, templateName, true);

        if (result.Success != true)
        {
            var searchedLocations = string.Join("\n", result.SearchedLocations);
            _logger.LogError("Could not find view: {viewPath}. Searched locations:\n{searchedLocations}", templateName, searchedLocations);
            throw new InvalidOperationException($"Could not find view: {templateName}. Searched locations:\n{searchedLocations}");
        }

        var view = result.View;

        using var writer = new StringWriter();
        var viewDataDict = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model,
        };

        var viewContext = new ViewContext(
            actionContext,
            view,
            viewDataDict,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            writer,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);

        return writer.ToString();
    }

    // For Razor Pages, set RouteData with "page" key and value as templateName
    private ActionContext GetActionContext()
    {
        var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider };
        return new ActionContext(httpContext, new RouteData { Values = { { "Controller", "EmailTemplates" } } }, new ControllerActionDescriptor { ControllerName = "EmailTemplates", ActionName = "Email" });
    }
}
