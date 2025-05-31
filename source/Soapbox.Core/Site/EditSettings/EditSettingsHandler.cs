namespace Soapbox.Application.Site.EditSettings;

using Microsoft.Extensions.Options;
using Soapbox.Application.Constants;
using Soapbox.Application.Settings;
using Soapbox.Application.Utils;
using Soapbox.Domain.Results;
using System.Text.Json;

public class EditSettingsHandler
{
    private readonly string _configPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Config);
    private readonly JsonWriterOptions _jsonWriterOptions = new() { Indented = true };

    private readonly SiteSettings _config;
    private readonly IViewEngineManager _viewEngineManager;

    public EditSettingsHandler(IOptionsSnapshot<SiteSettings> config, IViewEngineManager viewEngineManager)
    {
        _config = config.Value;
        _viewEngineManager = viewEngineManager;
    }

    public Result EditSettings(SiteSettings settings)
    {
        SaveToFile(settings, "site.json");

        if (_config.Theme != settings.Theme)
            _viewEngineManager.ClearCache();

        return Result.Success();
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
