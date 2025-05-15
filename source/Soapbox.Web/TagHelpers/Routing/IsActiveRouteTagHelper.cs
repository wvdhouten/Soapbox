namespace Soapbox.Web.TagHelpers.Routing;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

public enum ActiveRoutePrecision
{
    All,
    Action,
    Controller,
    Area
}

[HtmlTargetElement(Attributes = IsActiveRouteAttribute)]
public class IsActiveRouteTagHelper : TagHelper
{
    private const string IsActiveRouteAttribute = "is-active-route";

    private Dictionary<string, string> _routeValues = [];

    [HtmlAttributeName("asp-action")]
    public string? Action { get; set; }

    [HtmlAttributeName("asp-controller")]
    public string? Controller { get; set; }

    [HtmlAttributeName("asp-area")]
    public string? Area { get; set; }

    [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
    public Dictionary<string, string> RouteValues
    {
        get => _routeValues ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        set => _routeValues = value;
    }

    [HtmlAttributeName(IsActiveRouteAttribute)]
    public ActiveRoutePrecision Precision { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        if (ShouldBeActive())
            MakeActive(output);

        output.Attributes.RemoveAll(IsActiveRouteAttribute);
    }

    private bool ShouldBeActive()
    {
        var desiredArea = Area ?? string.Empty;
        var currentArea = ViewContext.RouteData.Values["Area"]?.ToString() ?? string.Empty;
        var desiredController = Controller ?? string.Empty;
        var currentController = ViewContext.RouteData.Values["Controller"]?.ToString();
        var desiredAction = Action ?? string.Empty;
        var currentAction = ViewContext.RouteData.Values["Action"]?.ToString();

        return Precision switch
        {
            ActiveRoutePrecision.Area => currentArea == desiredArea,
            ActiveRoutePrecision.Controller => currentArea == desiredArea
                && currentController == desiredController,
            ActiveRoutePrecision.Action => currentArea == desiredArea
                && currentController == desiredController
                && currentAction == desiredAction,
            // TODO: Add Route Values
            ActiveRoutePrecision.All
            or _ => currentArea == desiredArea
                && currentController == desiredController
                && currentAction == desiredAction,
        };
    }

    private static void MakeActive(TagHelperOutput output)
    {
        var value = "active";
        if (output.Attributes.TryGetAttribute("class", out var classAttribute))
            value = classAttribute.Value?.ToString() + " active" ?? "active";

        output.Attributes.SetAttribute("class", value);
    }
}
