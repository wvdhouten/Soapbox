namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.FileManagement;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Categories;
    using Soapbox.Web.Areas.Admin.Models.Media;
    using Soapbox.Web.Identity.Attributes;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator, UserRole.Editor)]
    public class MediaController : Controller
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
}
