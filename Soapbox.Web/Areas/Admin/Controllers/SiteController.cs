namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System.Threading;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.FileManagement;
    using Soapbox.Core.Identity;
    using Soapbox.Core.Settings;
    using Soapbox.Web.Identity.Attributes;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator)]
    public class SiteController : Controller
    {
        private readonly ConfigFileService _configFileService;

        public SiteController()
        {
            // TODO: Dependency Injection
            _configFileService = new ConfigFileService();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(nameof(Dashboard));
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings([FromServices] IOptionsSnapshot<SiteSettings> config)
        {
            return View(config.Value);
        }

        [HttpPost]
        public IActionResult Settings([FromForm] SiteSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return View(settings);
            }

            _configFileService.SaveToFile(settings, "site.json");

            // Wait for the settings to be written before redirecting.
            Thread.Sleep(250);

            return RedirectToAction("Settings");
        }
    }
}
