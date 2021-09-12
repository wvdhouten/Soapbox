namespace Soapbox.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Common;
    using Soapbox.Web.Models;
    using System.Diagnostics;

    [Route("[controller]/[action]")]
    public class PagesController : Controller
    {
        private readonly ILogger<PagesController> _logger;

        public PagesController(ILogger<PagesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("~/{page?}")]
        [HttpGet("~/Pages/{page?}")]
        public IActionResult Index([FromRoute] string page = null)
        {
            if (page is null)
            {
                return View();
            }

            return View(page);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
