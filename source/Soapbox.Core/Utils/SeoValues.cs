namespace Soapbox.Application.Utils;

using Soapbox.Application.Settings;
using Soapbox.Application.Constants;

public class SeoValues
{
    private readonly IDictionary<string, object?> _viewData;
    private readonly SiteSettings _settings;

    public string Title => GetViewDataValue(ViewConstants.PageTitle) != null ? $"{GetViewDataValue(ViewConstants.PageTitle)} - {_settings.Title}" : _settings.Title;

    public string Description => GetViewDataValue(ViewConstants.Description) ?? _settings.Description ?? string.Empty;

    public string Keywords => GetViewDataValue(ViewConstants.Keywords) ?? _settings.Keywords ?? string.Empty;

    public string Author => GetViewDataValue(ViewConstants.Author) ?? _settings.Owner ?? string.Empty;

    public string Owner => GetViewDataValue(ViewConstants.Owner) ?? _settings.Owner ?? string.Empty;

    public string? Image => GetViewDataValue(ViewConstants.Image);

    public string? Video => GetViewDataValue(ViewConstants.Video);

    public SeoValues(IDictionary<string, object?> viewData, SiteSettings settings)
    {
        _viewData = viewData;
        _settings = settings;
    }

    public static SeoValues Create(IDictionary<string, object?> viewData, SiteSettings settings)
        => new(viewData, settings);

    private string? GetViewDataValue(string key) => _viewData.TryGetValue(key, out var value) ? value?.ToString() : null;
}
