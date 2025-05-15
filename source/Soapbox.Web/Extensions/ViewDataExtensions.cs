namespace Soapbox.Web.Extensions;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Soapbox.Application.Settings;
using Soapbox.Web.Models;

public static class ViewDataExtensions
{
    public static SeoValues GetSeoValues(this ViewDataDictionary dictionary, SiteSettings settings) 
        => new(dictionary, settings);
}
