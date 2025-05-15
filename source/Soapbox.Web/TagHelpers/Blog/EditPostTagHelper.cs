namespace Soapbox.Web.TagHelpers.Blog;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Soapbox.Domain.Blog;

public class EditPostTagHelper : TagHelper
{
    private readonly IHtmlGenerator _generator;

    public Post Post { get; set; } = default!;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public EditPostTagHelper(IHtmlGenerator generator)
    {
        _generator = generator;
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        var routeValues = new Dictionary<string, object?> { { "id", Post.Id } };

        var builder = GetTagBuilder("Edit", routeValues);

        output.MergeAttributes(builder);

        return Task.Run(() => Process(context, output));
    }

    private TagBuilder GetTagBuilder(string action, Dictionary<string, object?> routeValues)
    {
        routeValues.Add("Area", "Admin");

        return _generator.GenerateActionLink(ViewContext, string.Empty, action, "Posts", null, null, null, routeValues, null);
    }
}
