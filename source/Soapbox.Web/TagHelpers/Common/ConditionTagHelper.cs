namespace Soapbox.Web.TagHelpers.Common;

using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement(Attributes = "condition")]
public class ConditionTagHelper : TagHelper
{
    public bool Condition { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        if (!Condition)
            output.SuppressOutput();
    }
}
