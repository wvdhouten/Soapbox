namespace Soapbox.Web.Areas.Admin.Models.Media;

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

public class UploadFilesViewModel
{
    public IEnumerable<IFormFile> Files { get; set; } = [];
}
