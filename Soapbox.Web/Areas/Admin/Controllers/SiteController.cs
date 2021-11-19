namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System.Threading;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.FileManagement;
    using Soapbox.Core.Settings;
    using Soapbox.Models;
    using Soapbox.Web.Identity.Attributes;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator)]
    public class SiteController : Controller
    {
        private readonly ConfigFileService _configFileService;
        private readonly ILogger<SiteController> _logger;

        public SiteController(ConfigFileService configFileService, ILogger<SiteController> logger)
        {
            _configFileService = configFileService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Settings([FromServices] IOptionsSnapshot<SiteSettings> config)
        {
            _logger.Log(LogLevel.Trace, "Loading site settings.");

            return View(config.Value);
        }

        [HttpPost]
        public IActionResult Settings([FromForm] SiteSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return View(settings);
            }

            _logger.Log(LogLevel.Trace, "Updating site settings.");

            // The redirect would happen before the file is fully updated, hence sleep.
            _configFileService.SaveToFile(settings, "site.json");
            Thread.Sleep(250);

            _logger.Log(LogLevel.Information, "Site settings updated.");

            return RedirectToAction(nameof(Settings));
        }
    }
}
