namespace Soapbox.Web.TagHelpers.Blog
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.TagHelpers;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    public class AccountLinkTagHelper : TagHelper
    {
        private readonly IHtmlGenerator _generator;

        public string Page { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public AccountLinkTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;

            var builder = _generator.GenerateActionLink(ViewContext, string.Empty, Page, "Account", null, null, null, null, null);

            output.MergeAttributes(builder);

            return Task.CompletedTask;
        }
    }
}