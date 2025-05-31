namespace Soapbox.Application.Media.Shared;
using System;
using Soapbox.Application.Constants;

public class MediaHandler
{
    protected string MediaPath { get; } = Path.Combine(Environment.CurrentDirectory, FolderNames.Content, FolderNames.Media);
}
