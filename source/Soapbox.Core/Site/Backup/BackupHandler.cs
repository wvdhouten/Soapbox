namespace Soapbox.Application.Site.DataBackup;

using Alkaline64.Injectable;
using Microsoft.Extensions.Logging;

using Soapbox.Application.Constants;
using Soapbox.Domain.Results;
using System;
using System.IO.Compression;

[Injectable]
public class BackupHandler
{
    private readonly ILogger<BackupHandler> _logger;

    public BackupHandler(ILogger<BackupHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Result<Stream> CreateArchiveBackup()
    {
        _logger.Log(LogLevel.Information, "Backup archive requested.");

        var contentPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Content);
        var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var filePath in Directory.GetFiles(contentPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(contentPath, filePath);
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Fastest);

                using var entryStream = entry.Open();
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fileStream.CopyTo(entryStream);
            }
        }

        memoryStream.Position = 0;

        return memoryStream;
    }
}
