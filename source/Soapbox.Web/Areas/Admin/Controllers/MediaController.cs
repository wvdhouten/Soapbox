namespace Soapbox.Web.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Soapbox.Application.FileManagement;
using Soapbox.Domain.Users;
using Soapbox.Web.Areas.Admin.Models.Media;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Identity.Attributes;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator, UserRole.Editor)]
public class MediaController : SoapboxControllerBase
{
    private readonly MediaFileService _fileService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(MediaFileService fileService, ILogger<MediaController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var model = _fileService.GetPage(page, pageSize);

        return View(model);
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Upload(UploadFilesViewModel model)
    {
        foreach (var file in model.Files)
        {
            if (file.Length > 0)
            {
                using var fileStream = file.OpenReadStream();
                _fileService.CreateOrUpdate(file.FileName, fileStream);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Delete(string name)
    {
        try
        {
            _fileService.Delete(name);
        }
        catch
        {
            // TODO
        }

        return RedirectToAction(nameof(Index));
    }
}
