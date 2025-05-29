namespace Soapbox.Web.Models;

using Soapbox.Application.Settings;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Soapbox.Application;

public class SeoValues
{
    private readonly ViewDataDictionary _viewData;
    private readonly SiteSettings _settings;

    public string Title => GetViewDataValue(Constants.PageTitle) != null ? $"{GetViewDataValue(Constants.PageTitle)} - {_settings.Title}" : _settings.Title;

    public string Description => GetViewDataValue(Constants.Description) ?? _settings.Description ?? string.Empty;

    public string Keywords => GetViewDataValue(Constants.Keywords) ?? _settings.Keywords ?? string.Empty;

    public string Author => GetViewDataValue(Constants.Author) ?? _settings.Owner ?? string.Empty;

    public string Owner => GetViewDataValue(Constants.Owner) ?? _settings.Owner ?? string.Empty;

    public string? Image => GetViewDataValue(Constants.Image);

    public string? Video => GetViewDataValue(Constants.Video);

    public SeoValues(ViewDataDictionary viewData, SiteSettings settings)
    {
        _viewData = viewData;
        _settings = settings;
    }

    public static SeoValues Create(ViewDataDictionary viewData, SiteSettings settings)
        => new(viewData, settings);

    private string? GetViewDataValue(string key) => _viewData?[key]?.ToString();
}
