namespace Soapbox.Application.Media;
using System;
using Soapbox.Application.Constants;

internal static class MediaInfo
{
    internal static string FilesPath { get; } = Path.Combine(Environment.CurrentDirectory, FolderNames.Content, FolderNames.Media);
}
