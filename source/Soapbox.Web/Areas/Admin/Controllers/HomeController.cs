namespace Soapbox.Web.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Users;
using Soapbox.Web.Attributes;
using Soapbox.Web.Controllers.Base;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator)]
public class HomeController : SoapboxControllerBase
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogTrace("Redirecting to post administration.");

        return RedirectToAction("Index", "Posts");
    }
}
