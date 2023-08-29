namespace Soapbox.Core.Email
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Email.Abstractions;

    public class RazorEmailRenderer : IEmailRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RazorEmailRenderer> _logger;

        public RazorEmailRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            ILogger<RazorEmailRenderer> logger)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        public async Task<string> Render<TModel>(string templateName, TModel model)
        {
            var actionContext = GetActionContext();

            // Note: You can also support multiple languages by separating each locale into a folder
            var viewPath = $"/Views/EmailTemplates/{templateName}.cshtml";
            var result = _viewEngine.GetView(null, viewPath, true);

            if (result.Success != true)
            {
                var searchedLocations = string.Join("\n", result.SearchedLocations);
                throw new InvalidOperationException($"Could not find this view: {viewPath}. Searched locations:\n{searchedLocations}");
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
    }
}
