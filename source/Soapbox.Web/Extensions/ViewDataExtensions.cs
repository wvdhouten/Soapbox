namespace Soapbox.Web.Extensions
{
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Soapbox.Core.Settings;

    public static class ViewDataExtensions
    {
        public static SeoValues GetSeoValues(this ViewDataDictionary dictionary, SiteSettings settings)
        {
            return new SeoValues(dictionary, settings);
        }
    }
}
