namespace Soapbox.Web.Controllers
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Common;
    using Soapbox.Web.Models;

    public class PagesController : Controller
    {
        private readonly ILogger<PagesController> _logger;

        public PagesController(ILogger<PagesController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            switch (statusCode)
            {
                case 404:
                    return View("NotFound");
            }

            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // TODO: Better public exception management.
            if (exceptionHandlerPathFeature?.Error is InvalidOperationException)
            {
                model.Message = exceptionHandlerPathFeature.Error.Message;
            }

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Offline()
        {
            return View();
        }
    }
}
