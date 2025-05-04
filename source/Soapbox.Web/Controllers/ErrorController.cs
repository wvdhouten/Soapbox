namespace Soapbox.Web.Controllers
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Web.Controllers.Base;
    using Soapbox.Web.Models;

    public class ErrorController : SoapboxControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(int? statusCode = null)
        {
            return statusCode switch
            {
                404 => GetNotFoundView(),
                403 => GetAccessDeniedView(),
                _ => GetGenericErrorView(),
            };
        }

        private ViewResult GetNotFoundView()
        {
            var feature = Request.HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var path = feature?.OriginalPath;

            return View("NotFound", new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier)
            {
                RequestedUrl = path,
                RedirectUrl = HttpContext.Request.GetDisplayUrl()
            });
        }

        private ViewResult GetAccessDeniedView()
        {
            return View("AccessDenied", new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier));
        }

        private ViewResult GetGenericErrorView()
        {
            var model = new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error is InvalidOperationException)
                model.Message = exceptionHandlerPathFeature.Error.Message;

            return View("Error", model);
        }

        [HttpGet]
        public IActionResult Offline()
        {
            return View();
        }
    }
}
