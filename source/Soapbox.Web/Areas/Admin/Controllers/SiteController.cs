namespace Soapbox.Web.Areas.Admin.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Soapbox.Application.Settings;
using Soapbox.Application.Site.Backup;
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
        [FromServices] RestartSiteHandler restartSiteHandler,
        [FromForm] SiteSettings settings,
        string action)
    {
        if (!ModelState.IsValid)
            return View(settings);

        var result = handler.EditSettings(settings);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Settings saved successfully.").View(settings),
            _ => SomethingWentWrong(),
        };
    }

    [HttpPost]
    public void Restart([FromServices] RestartSiteHandler handler) => handler.RestartSite();

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
