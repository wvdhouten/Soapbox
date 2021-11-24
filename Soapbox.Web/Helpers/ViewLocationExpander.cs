namespace Soapbox.Web.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Settings;

    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string ThemeLocation = "/Themes/{0}/{1}";

        private readonly IOptionsMonitor<SiteSettings> _settings;

        public ViewLocationExpander(IOptionsMonitor<SiteSettings> settings)
        {
            _settings = settings;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var theme = _settings.CurrentValue.Theme;
            theme = !string.IsNullOrEmpty(theme) ? theme : "Default";
            var themeLocation = string.Format(ThemeLocation, theme, "{0}.cshtml");

            return viewLocations.Prepend(themeLocation);
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
