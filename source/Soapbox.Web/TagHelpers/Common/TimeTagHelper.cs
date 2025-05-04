namespace Soapbox.Web.TagHelpers.Common;

using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("time", Attributes = "value")]
public class TimeTagHelper : TagHelper
{
    public DateTime Value { get; set; }

    // TODO: Default from site settings.
    public string Format { get; set; } = "MMM d, yyyy";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("datetime", Value.ToString("s"));
        output.Content.SetContent(Value.ToString(Format));
    }
}
