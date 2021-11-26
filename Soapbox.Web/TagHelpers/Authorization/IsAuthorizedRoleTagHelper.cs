namespace DasBlog.Web.TagHelpers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Soapbox.Models;

    [HtmlTargetElement(Attributes = "is-authorized")]
    public class IsAuthorizedRoleTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<SoapboxUser> _signInManager;

        public IsAuthorizedRoleTagHelper(IHttpContextAccessor httpContextAccessor, SignInManager<SoapboxUser> signInManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
        }

        // TODO: Add policies
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            var user = _httpContextAccessor.HttpContext.User;

            if (!_signInManager.IsSignedIn(user))
            {
                output.SuppressOutput();
            }
        }
    }
}
