namespace Soapbox.Application.FileManagement;

using System;
using System.IO;
using System.Text.Json;
using Alkaline64.Injectable;
using Soapbox.Application.Constants;

[Injectable]
public class ConfigFileService
{
    private readonly string _configPath;
    private readonly JsonWriterOptions _jsonWriterOptions = new() { Indented = true };

    public ConfigFileService()
    {
        _configPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Config);
    }

    public void SaveToFile<TConfig>(TConfig config, string fileName)
    {
        var filePath = Path.Combine(_configPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new Utf8JsonWriter(stream, _jsonWriterOptions);
        writer.WriteStartObject();
        writer.WritePropertyName(typeof(TConfig).Name);

        JsonSerializer.Serialize(writer, config);

        writer.WriteEndObject();
        writer.Flush();
    }
}
