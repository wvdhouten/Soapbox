namespace Soapbox.Application.Media.ListFiles;

using Alkaline64.Injectable;
using Soapbox.Application.Media;
using Soapbox.Application.Utils;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Common;
using Soapbox.Domain.Results;

[Injectable]
public class ListFilesHandler
{
    public Result<IPagedList<Media>> GetPage(int page = 1, int pageSize = 25)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(MediaInfo.FilesPath);
            var files = directoryInfo.GetFiles();
            var media = files.Select(f => new Media { Name = f.Name, Size = f.Length, ModifiedOn = f.LastWriteTimeUtc }).AsQueryable();
            return Result.Success(media.GetPaged(page, pageSize));
        }
        catch
        {
            return Error.Unknown();
        }
    }
}
