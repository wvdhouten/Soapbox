namespace Soapbox.Web.Controllers
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Common;
    using Soapbox.Web.Models;
    using System;
    using System.Diagnostics;

    [Route("[controller]/[action]")]
    public class PagesController : Controller
    {
        private readonly ILogger<PagesController> _logger;

        public PagesController(ILogger<PagesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("~/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("~/Pages/{page}")]
        public IActionResult Page([FromRoute] string page = null)
        {
            if (page is null)
            {
                return View();
            }

            return View(page);
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

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Offline()
        {
            ViewData[Constants.Title] = Resources.Offline;

            return View();
        }
    }
}
