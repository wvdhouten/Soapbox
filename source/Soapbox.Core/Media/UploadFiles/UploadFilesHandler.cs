namespace Soapbox.Application.Media.UploadFiles;

using Alkaline64.Injectable;
using Soapbox.Application.Media;
using Soapbox.Domain.Results;
using System.IO;

[Injectable]
public class UploadFilesHandler
{
    public Result UploadFiles(UploadFilesRequest request)
    {
        foreach (var file in request.Files)
        {
            if (file.Length is 0)
                continue;

            using var fileStream = file.OpenReadStream();
            var result = UploadFile(file.FileName, fileStream);
            if (result.IsFailure)
                return result;
        }

        return Result.Success();
    }

    public Result UploadFile(string name, Stream stream)
    {
        try
        {
            var filePath = Path.Combine(MediaInfo.FilesPath, name);
            using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            stream.CopyTo(fileStream);
            fileStream.Flush();

            return Result.Success();
        }
        catch
        {
            return Error.Unknown();
        }
    }
}
