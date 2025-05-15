namespace Soapbox.Web.TagHelpers.Routing;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

public class PageLinkTagHelper : TagHelper
{
    private readonly IHtmlGenerator _generator;

    public string Page { get; set; } = string.Empty;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public PageLinkTagHelper(IHtmlGenerator generator)
    {
        _generator = generator;
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        var builder = GetTagBuilder(!string.IsNullOrEmpty(Page) ? Page : "Index", null);

        output.MergeAttributes(builder);

        return Task.CompletedTask;
    }

    private TagBuilder GetTagBuilder(string action, IDictionary<string, object>? routeValues)
    {
        routeValues ??= new Dictionary<string, object>();
        routeValues.TryAdd("Area", "");

        return _generator.GenerateActionLink(ViewContext, string.Empty, action, "Pages", null, null, null, routeValues, null);
    }
}