namespace Soapbox.Web.Helpers;

using System.Collections.Generic;
using System.Linq;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Soapbox.Application.Constants;
using Soapbox.Application.Settings;

[Injectable(Lifetime.Singleton)]
public class ViewLocationExpander : IViewLocationExpander
{
    private const string ContentViewLocation = $"/{FolderNames.Content}/{FolderNames.Views}/{{0}}.cshtml";
    private const string ThemedViewLocation = $"/{FolderNames.Themes}/{{0}}/{{1}}.cshtml";

    private readonly IOptionsMonitor<SiteSettings> _settings;

    public ViewLocationExpander(IOptionsMonitor<SiteSettings> settings)
    {
        _settings = settings;
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        // TODO: Some inconsistencies might occur here. Must be verified.

        var contentViewLocations = new[] {
            string.Format(ContentViewLocation, "{1}/{0}"),
            string.Format(ContentViewLocation, "{0}")
        };

        string theme = GetTheme();
        var themedViewLocations = new[] {
            string.Format(ThemedViewLocation, theme, "{1}/{0}"),
            string.Format(ThemedViewLocation, theme, "{0}")
        };

        return contentViewLocations.Concat(themedViewLocations).Concat(viewLocations);
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    private string GetTheme()
    {
        var theme = _settings.CurrentValue.Theme;
        return !string.IsNullOrEmpty(theme) ? theme : "Default";
    }
}
