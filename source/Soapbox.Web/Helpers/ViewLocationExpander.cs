namespace Soapbox.Web.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.Options;
    using Soapbox.Application.Settings;

    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string ThemedViewLocation = "/Themes/{0}/{1}.cshtml";

        private readonly IOptionsMonitor<SiteSettings> _settings;

        public ViewLocationExpander(IOptionsMonitor<SiteSettings> settings)
        {
            _settings = settings;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var theme = _settings.CurrentValue.Theme;
            theme = !string.IsNullOrEmpty(theme) ? theme : "Default";

            var themedViewLocations = new List<string>
            {
                string.Format(ThemedViewLocation, theme, "{1}/{0}"),
                string.Format(ThemedViewLocation, theme, "{0}"),
            };

            return themedViewLocations.Concat(viewLocations);
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
