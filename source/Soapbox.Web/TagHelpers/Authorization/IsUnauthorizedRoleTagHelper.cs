namespace DasBlog.Web.TagHelpers
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Soapbox.Models;

    [HtmlTargetElement(Attributes = IsUnuthorizedAttribute)]
    public class IsUnauthorizedRoleTagHelper : TagHelper
	{
        private const string IsUnuthorizedAttribute = "is-unauthorized";

        private readonly SignInManager<SoapboxUser> _signInManager;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public IsUnauthorizedRoleTagHelper(SignInManager<SoapboxUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            var user = ViewContext.HttpContext.User;

            if (_signInManager.IsSignedIn(user))
            {
                output.SuppressOutput();
            }

            output.Attributes.RemoveAll(IsUnuthorizedAttribute);
        }
    }
}
