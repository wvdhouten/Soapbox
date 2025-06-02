namespace Soapbox.Web.Areas.Admin.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Soapbox.Application.Settings;
using Soapbox.Application.Site.DataBackup;
using Soapbox.Application.Site.EditSettings;
using Soapbox.Application.Site.Restart;
using Soapbox.Domain.Users;
using Soapbox.Web.Attributes;
using Soapbox.Web.Controllers.Base;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator)]
public class SiteController : SoapboxControllerBase
{
    [HttpGet]
    public IActionResult Settings([FromServices] IOptionsSnapshot<SiteSettings> config) => View(config.Value);

    [HttpPost]
    public IActionResult Settings(
        [FromServices] EditSettingsHandler handler,
        [FromForm] SiteSettings settings)
    {
        if (!ModelState.IsValid)
            return View(settings);

        var result = handler.EditSettings(settings);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Settings saved successfully.").RedirectToAction(nameof(settings)),
            _ => SomethingWentWrong(),
        };
    }

    [HttpPost]
    public IActionResult Restart([FromServices] RestartSiteHandler handler)
    {
        var result = handler.RestartSite();
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Site will restart.").RedirectToAction(nameof(Settings)),
            _ => SomethingWentWrong()
        };
        
    }

    [HttpGet]
    public IActionResult Backup([FromServices] BackupHandler handler)
    {
        var result = handler.CreateArchiveBackup();
        return result switch
        {
            { IsSuccess: true, Value: var stream } => File(stream, "application/zip", $"Backup-Soapbox-{DateTime.UtcNow:yyyy-MM-dd}.zip"),
            _ => SomethingWentWrong(),
        };
    }
}
