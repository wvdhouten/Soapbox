namespace Soapbox.Application.Media.UploadFiles;

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

public class UploadFilesRequest
{
    public IEnumerable<IFormFile> Files { get; set; } = [];
}
