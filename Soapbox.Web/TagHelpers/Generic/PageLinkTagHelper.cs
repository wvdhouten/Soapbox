namespace Soapbox.Web.TagHelpers.Generic
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.TagHelpers;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    public class PageLinkTagHelper : TagHelper
    {
        private readonly IHtmlGenerator _generator;

        public string Page { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public PageLinkTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;

            var builder = _generator.GenerateActionLink(ViewContext, string.Empty, Page, "Pages", null, null, null, null, null);

            output.MergeAttributes(builder);

            return Task.CompletedTask;
        }
    }
}