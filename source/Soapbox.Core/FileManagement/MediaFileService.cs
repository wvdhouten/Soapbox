namespace Soapbox.Application.FileManagement;

using System;
using System.IO;
using System.Linq;
using Soapbox.Application.Utils;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Common;

public class MediaFileService
{
    private readonly string _mediaPath;

    public MediaFileService()
    {
        _mediaPath = Path.Combine(Environment.CurrentDirectory, "Content", "Files");
    }

    public IPagedList<Media> GetPage(int page = 1, int pageSize = 25)
    {
        var directoryInfo = new DirectoryInfo(_mediaPath);
        var files = directoryInfo.GetFiles();
        var media = files.Select(f => new Media { Name = f.Name, Size = f.Length, ModifiedOn = f.LastWriteTimeUtc }).AsQueryable();
        var total = media.Count();
        return media.GetPaged(page, pageSize);
    }

    public void CreateOrUpdate(string name, Stream stream)
    {
        var filePath = Path.Combine(_mediaPath, name);
        using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        stream.CopyTo(fileStream);
        fileStream.Flush();
    }

    public void Delete(string name)
    {
        var filePath = Path.Combine(_mediaPath, name);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
