namespace Soapbox.Web.Areas.Admin.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.FileManagement;
    using Soapbox.Core.Identity;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Settings;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator)]
    public class SiteController : Controller
    {
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

            // TODO: Dependency Injection
            var service = new ConfigFileService();
            service.SaveToFile(settings, "site.json");

            return View(settings);
        }
    }
}
