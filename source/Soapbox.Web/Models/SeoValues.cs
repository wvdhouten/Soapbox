namespace Soapbox.Core.Settings
{
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Soapbox.Core.Common;

    public class SeoValues
    {
        private readonly ViewDataDictionary _viewData;
        private readonly SiteSettings _settings;

        public string Title => GetViewDataValue(Constants.PageTitle) != null ? $"{GetViewDataValue(Constants.PageTitle)} - {_settings.Title}" : _settings.Title;

        public string Description => GetViewDataValue(Constants.Description) ?? _settings.Description;

        public string Keywords => GetViewDataValue(Constants.Keywords) ?? _settings.Keywords;

        public string Author => GetViewDataValue(Constants.Author) ?? _settings.Owner;

        public string Owner => GetViewDataValue(Constants.Owner) ?? _settings.Owner;

        public string Image => GetViewDataValue(Constants.Image);

        public string Video => GetViewDataValue(Constants.Video);

        public SeoValues(ViewDataDictionary viewData, SiteSettings settings)
        {
            _viewData = viewData;
            _settings = settings;
        }

        private string GetViewDataValue(string key)
        {
            return _viewData?[key]?.ToString();
        }
    }
}
