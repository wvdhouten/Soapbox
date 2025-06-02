namespace Soapbox.Application.Media.DeleteFile;

using Alkaline64.Injectable;
using Soapbox.Application.Media.Shared;
using Soapbox.Domain.Results;

[Injectable]
public class DeleteFileHandler
{
    public Result DeleteFile(string name)
    {
        try
        {
            var filePath = Path.Combine(MediaInfo.FilesPath, name);
            if (File.Exists(filePath))
                File.Delete(filePath);

            return Result.Success();
        }
        catch
        {
            return Error.Unknown();
        }
    }
}
