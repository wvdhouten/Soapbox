namespace Soapbox.Web.Areas.Admin.Controllers;

using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Soapbox.Application.FileManagement;
using Soapbox.Application.Settings;
using Soapbox.Domain.Users;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Extensions;
using Soapbox.Web.Identity.Attributes;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator)]
public class SiteController : SoapboxControllerBase
{
    private readonly ConfigFileService _configFileService;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<SiteController> _logger;

    public SiteController(ConfigFileService configFileService, IHostApplicationLifetime appLifetime, ILogger<SiteController> logger)
    {
        _configFileService = configFileService;
        _appLifetime = appLifetime;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Settings([FromServices] IOptionsSnapshot<SiteSettings> config)
    {
        _logger.Log(LogLevel.Trace, "Loading site settings.");

        return View(config.Value);
    }

    [HttpPost]
    public IActionResult Settings([FromForm] SiteSettings settings, [FromServices] IOptionsSnapshot<SiteSettings> config, [FromServices] IRazorViewEngine viewEngine, string action)
    {
        if (action == nameof(Restart))
        {
            return Restart();
        }

        if (!ModelState.IsValid)
        {
            return View(settings);
        }

        _logger.Log(LogLevel.Trace, "Updating site settings.");

        _configFileService.SaveToFile(settings, "site.json");

        _logger.Log(LogLevel.Information, "Site settings updated.");

        if (config.Value.Theme != settings.Theme)
            viewEngine.TryInvokeMethod(typeof(RazorViewEngine), "ClearCache");

        StatusMessage = "Settings saved successfully.";

        return View(settings);
    }

    [HttpPost]
    public IActionResult Restart()
    {
        _appLifetime.StopApplication();

        throw new Exception("Application will restart.");
    }

    [HttpGet]
    public IActionResult Backup()
    {
        _logger.Log(LogLevel.Information, "Backup downloaded.");

        var contentPath = Path.Combine(Environment.CurrentDirectory, "Content");
        var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var filePath in Directory.GetFiles(contentPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(contentPath, filePath);
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Fastest);

                using var entryStream = entry.Open();
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fileStream.CopyTo(entryStream);
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream, "application/zip", $"Backup-Soapbox-{DateTime.UtcNow:yyyy-MM-dd}.zip");
    }
}
