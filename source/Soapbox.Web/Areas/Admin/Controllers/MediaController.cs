namespace Soapbox.Web.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Media.DeleteFile;
using Soapbox.Application.Media.ListFiles;
using Soapbox.Application.Media.UploadFiles;
using Soapbox.Domain.Users;
using Soapbox.Web.Attributes;
using Soapbox.Web.Controllers.Base;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator, UserRole.Editor)]
public class MediaController : SoapboxControllerBase
{
    [HttpGet]
    public IActionResult Index(
        [FromServices] ListFilesHandler handler,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = handler.GetPage(page, pageSize);
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost]
    public IActionResult Upload(
        [FromServices] UploadFilesHandler handler, 
        [FromForm] UploadFilesRequest request)
    {
        var result = handler.UploadFiles(request);
        return result switch
        {
            { IsSuccess: true } => RedirectToAction(nameof(Index)),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpPost]
    public IActionResult Delete(
        [FromServices] DeleteFileHandler handler,
        [FromRoute] string id)
    {
        var result = handler.DeleteFile(id);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage($"{id} deleted.").RedirectToAction(nameof(Index)),
            _ => BadRequest("Something went wrong.")
        };
    }
}
