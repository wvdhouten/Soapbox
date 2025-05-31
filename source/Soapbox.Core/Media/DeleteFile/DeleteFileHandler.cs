namespace Soapbox.Application.Media.DeleteFile;
using Soapbox.Application.Media.Shared;
using Soapbox.Domain.Results;

public class DeleteFileHandler : MediaHandler
{
    public Result DeleteFile(string name)
    {
        try
        {
            var filePath = Path.Combine(MediaPath, name);
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
