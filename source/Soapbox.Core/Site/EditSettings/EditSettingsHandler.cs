namespace Soapbox.Application.Site.EditSettings;

using Alkaline64.Injectable;
using Microsoft.Extensions.Options;
using Soapbox.Application.Constants;
using Soapbox.Application.Settings;
using Soapbox.Application.Utils;
using Soapbox.Domain.Results;
using System.Text.Json;

[Injectable]
public class EditSettingsHandler
{
    private readonly string _configPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Config);
    private readonly JsonWriterOptions _jsonWriterOptions = new() { Indented = true };

    private readonly IViewEngineManager _viewEngineManager;

    public EditSettingsHandler(IViewEngineManager viewEngineManager)
    {
        _viewEngineManager = viewEngineManager;
    }

    public Result EditSettings(SiteSettings settings)
    {
        SaveToFile(settings, "site.json");

        _viewEngineManager.ClearCache();

        return Result.Success();
    }

    private void SaveToFile<TConfig>(TConfig config, string fileName)
    {
        var filePath = Path.Combine(_configPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new Utf8JsonWriter(stream, _jsonWriterOptions);
        writer.WriteStartObject();
        writer.WritePropertyName(typeof(TConfig).Name);

        JsonSerializer.Serialize(writer, config);

        writer.WriteEndObject();
        writer.Flush();

        // This sleep will (most likely) wait until the file is truly written, and result in the settings being available immediately.
        Thread.Sleep(500);
    }
}
